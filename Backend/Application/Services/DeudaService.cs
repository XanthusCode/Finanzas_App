using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class DeudaService(IDeudaRepository repo, IIngresosRepository ingresosRepo, IGastosRepository gastosRepo, IMapper mapper)
    {
        private readonly IDeudaRepository _repo   = repo;
        private readonly IMapper          _mapper = mapper;

        public async Task<IEnumerable<DeudaDto>> GetAllAsync(Guid userId)
        {
            var items   = await _repo.GetAllAsync(userId);
            var pagados = await _repo.GetMontosPagadosAsync(userId);

            var dtos = _mapper.Map<List<DeudaDto>>(items);
            foreach (var dto in dtos)
            {
                var pagado = pagados.GetValueOrDefault(dto.Id);
                dto.MontoPagado   = pagado;
                dto.MontoRestante = Math.Max(dto.Monto - pagado, 0);
            }
            return dtos;
        }

        public async Task<DeudaDto> CreateAsync(CrearDeudaDto dto, Guid userId)
        {
            var deuda = _mapper.Map<Deuda>(dto);
            deuda.UserId = userId;
            var creada = await _repo.CreateAsync(deuda);
            return _mapper.Map<DeudaDto>(creada);
        }

        public async Task<DeudaDto?> MarcarPagadaAsync(Guid id, Guid userId)
        {
            var deuda = await _repo.GetByIdAsync(id, userId);
            if (deuda is null) return null;

            var estabaPagada = deuda.Pagada;
            deuda.Pagada = !deuda.Pagada;

            if (!estabaPagada && deuda.Pagada)
            {
                var pagados     = await _repo.GetMontosPagadosAsync(userId);
                var totalPagado = pagados.GetValueOrDefault(id);
                var restante    = deuda.Monto - totalPagado;
                var hoy         = DateTime.UtcNow;

                if (restante > 0 && deuda.Tipo == TipoDeuda.ME_DEBEN)
                {
                    await ingresosRepo.CreateAsync(new Ingreso
                    {
                        Concepto = $"Deuda cobrada: {deuda.Persona}",
                        Monto = restante,
                        Mes = hoy.Month,
                        Anio = hoy.Year,
                        UserId = userId,
                        DeudaId = deuda.Id
                    });
                }
                else if (restante > 0 && deuda.Tipo == TipoDeuda.LE_DEBO)
                {
                    await gastosRepo.CreateAsync(new Gasto
                    {
                        Categoria = "Deuda",
                        Detalle = $"Deuda pagada: {deuda.Persona}",
                        Monto = restante,
                        Tipo = TipoGasto.VARIABLE,
                        Mes = hoy.Month,
                        Anio = hoy.Year,
                        UserId = userId,
                        DeudaId = deuda.Id
                    });
                }
            }
            else if (estabaPagada && !deuda.Pagada)
            {
                await ingresosRepo.DeleteByDeudaIdAsync(id);
                await gastosRepo.DeleteByDeudaIdAsync(id);
            }

            var actualizada = await _repo.UpdateAsync(deuda);
            return _mapper.Map<DeudaDto>(actualizada);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var deuda = await _repo.GetByIdAsync(id, userId);
            if (deuda is null) return false;
            await ingresosRepo.DeleteByDeudaIdAsync(id);
            await gastosRepo.DeleteByDeudaIdAsync(id);
            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<DeudaDto?> RegistrarAbonoAsync(Guid deudaId, CrearAbonoDto dto, Guid userId)
        {
            var deuda = await _repo.GetByIdAsync(deudaId, userId);
            if (deuda is null) return null;

            await _repo.AddAbonoAsync(new Abono { DeudaId = deudaId, Monto = dto.Monto });

            if (deuda.Tipo == TipoDeuda.ME_DEBEN)
            {
                var hoy = DateTime.UtcNow;
                await ingresosRepo.CreateAsync(new Ingreso
                {
                    Concepto = $"Deuda cobrada: {deuda.Persona}",
                    Monto = dto.Monto,
                    Mes = hoy.Month,
                    Anio = hoy.Year,
                    UserId = userId
                });
            }
            else if (deuda.Tipo == TipoDeuda.LE_DEBO)
            {
                var hoy = DateTime.UtcNow;
                await gastosRepo.CreateAsync(new Gasto
                {
                    Categoria = "Deuda",
                    Detalle = $"Deuda pagada: {deuda.Persona}",
                    Monto = dto.Monto,
                    Tipo = TipoGasto.VARIABLE,
                    Mes = hoy.Month,
                    Anio = hoy.Year,
                    UserId = userId
                });
            }

            var pagados      = await _repo.GetMontosPagadosAsync(userId);
            var totalPagado  = pagados.GetValueOrDefault(deudaId);

            if (totalPagado >= deuda.Monto && !deuda.Pagada)
            {
                deuda.Pagada = true;
                deuda = await _repo.UpdateAsync(deuda);
            }

            var result = _mapper.Map<DeudaDto>(deuda);
            result.MontoPagado   = totalPagado;
            result.MontoRestante = Math.Max(deuda.Monto - totalPagado, 0);
            return result;
        }

        public async Task<IEnumerable<AbonoDto>?> GetAbonosAsync(Guid deudaId, Guid userId)
        {
            var deuda = await _repo.GetByIdAsync(deudaId, userId);
            if (deuda is null) return null;

            var abonos = await _repo.GetAbonosAsync(deudaId);
            return _mapper.Map<IEnumerable<AbonoDto>>(abonos);
        }
    }
}
