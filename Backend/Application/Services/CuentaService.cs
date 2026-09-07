using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class CuentaService(ICuentaRepository repo, IMapper mapper)
    {
        private readonly ICuentaRepository _repo   = repo;
        private readonly IMapper           _mapper = mapper;

        public async Task<IEnumerable<CuentaDto>> GetAllAsync(Guid userId)
        {
            var items  = await _repo.GetAllAsync(userId);
            var saldos = await _repo.GetSaldosAsync(userId);

            var dtos = _mapper.Map<List<CuentaDto>>(items);
            foreach (var dto in dtos)
                dto.Saldo = saldos.GetValueOrDefault(dto.Id);

            return dtos;
        }

        public async Task<CuentaDto> CreateAsync(CrearCuentaDto dto, Guid userId)
        {
            var cuenta = _mapper.Map<Cuenta>(dto);
            cuenta.UserId = userId;
            var creada = await _repo.CreateAsync(cuenta);

            if (dto.SaldoInicial != 0)
                await _repo.AddMovimientoAsync(new MovimientoCuenta
                {
                    CuentaId = creada.Id,
                    Monto = dto.SaldoInicial,
                    Descripcion = "Saldo inicial"
                });

            var result = _mapper.Map<CuentaDto>(creada);
            result.Saldo = dto.SaldoInicial;
            return result;
        }

        public async Task<CuentaDto?> UpdateAsync(Guid id, EditarCuentaDto dto, Guid userId)
        {
            var cuenta = await _repo.GetByIdAsync(id, userId);
            if (cuenta is null) return null;

            cuenta.Nombre = dto.Nombre;
            cuenta.Tipo   = Enum.Parse<TipoCuenta>(dto.Tipo);
            var actualizada = await _repo.UpdateAsync(cuenta);

            var saldos = await _repo.GetSaldosAsync(userId);
            var result = _mapper.Map<CuentaDto>(actualizada);
            result.Saldo = saldos.GetValueOrDefault(id);
            return result;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var cuenta = await _repo.GetByIdAsync(id, userId);
            if (cuenta is null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<CuentaDto?> RegistrarMovimientoAsync(Guid cuentaId, CrearMovimientoCuentaDto dto, Guid userId)
        {
            var cuenta = await _repo.GetByIdAsync(cuentaId, userId);
            if (cuenta is null) return null;

            await _repo.AddMovimientoAsync(new MovimientoCuenta
            {
                CuentaId = cuentaId,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion
            });

            var saldos = await _repo.GetSaldosAsync(userId);
            var result = _mapper.Map<CuentaDto>(cuenta);
            result.Saldo = saldos.GetValueOrDefault(cuentaId);
            return result;
        }

        public async Task<IEnumerable<MovimientoCuentaDto>?> GetMovimientosAsync(Guid cuentaId, Guid userId)
        {
            var cuenta = await _repo.GetByIdAsync(cuentaId, userId);
            if (cuenta is null) return null;

            var movimientos = await _repo.GetMovimientosAsync(cuentaId);
            return _mapper.Map<IEnumerable<MovimientoCuentaDto>>(movimientos);
        }

        // Historial unificado: movimientos manuales + ingresos y gastos asignados
        // a la cuenta. Explica el saldo, que se calcula sumando las tres fuentes.
        public async Task<IEnumerable<LineaCuentaDto>?> GetLedgerAsync(Guid cuentaId, Guid userId)
        {
            var cuenta = await _repo.GetByIdAsync(cuentaId, userId);
            if (cuenta is null) return null;

            return await _repo.GetLedgerAsync(cuentaId, userId);
        }
    }
}
