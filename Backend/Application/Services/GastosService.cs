using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class GastosService(IGastosRepository repo, IMapper mapper)
    {
        private readonly IGastosRepository _repo = repo;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<GastoDto>> GetByMesAsync(int mes, int anio, Guid userId)
        {
            var gastos = await _repo.GetByMesAsync(mes, anio, userId);
            return _mapper.Map<IEnumerable<GastoDto>>(gastos);
        }

        public async Task<GastoDto> CreateAsync(CrearGastoDto dto, Guid userId)
        {
            var numCuotas = dto.NumCuotas ?? 1;

            if (numCuotas <= 1)
            {
                var gasto = _mapper.Map<Gasto>(dto);
                gasto.UserId = userId;
                return _mapper.Map<GastoDto>(await _repo.CreateAsync(gasto));
            }

            // Genera N gastos (uno por mes) con el mismo origenId
            var origenId = Guid.NewGuid();
            Gasto? primero = null;

            for (int i = 0; i < numCuotas; i++)
            {
                int mes  = dto.Mes + i;
                int anio = dto.Anio;
                while (mes > 12) { mes -= 12; anio++; }

                var g = _mapper.Map<Gasto>(dto);
                g.Id            = i == 0 ? origenId : Guid.NewGuid();
                g.UserId        = userId;
                g.Mes           = mes;
                g.Anio          = anio;
                g.NumCuotas     = numCuotas;
                g.CuotaActual   = i + 1;
                g.GastoOrigenId = origenId;

                var creado = await _repo.CreateAsync(g);
                primero ??= creado;
            }

            return _mapper.Map<GastoDto>(primero!);
        }

        public async Task<GastoDto?> UpdateAsync(Guid id, CrearGastoDto dto, Guid userId)
        {
            var existente = await _repo.GetByIdAsync(id, userId);
            if (existente is null) return null;

            _mapper.Map(dto, existente);
            var actualizado = await _repo.UpdateAsync(existente);
            return _mapper.Map<GastoDto>(actualizado);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var existente = await _repo.GetByIdAsync(id, userId);
            if (existente is null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<int> ImportarAsync(IEnumerable<CrearGastoDto> dtos, int mes, int anio, Guid userId)
        {
            int count = 0;
            foreach (var dto in dtos)
            {
                var gasto = _mapper.Map<Gasto>(dto);
                gasto.UserId = userId;
                gasto.Mes    = mes;
                gasto.Anio   = anio;
                await _repo.CreateAsync(gasto);
                count++;
            }
            return count;
        }

        public async Task<IEnumerable<ResumenCategoriaDto>> GetResumenCategoriaAsync(int mes, int anio, Guid userId) =>
            await _repo.GetResumenCategoriaAsync(mes, anio, userId);

        public async Task<IEnumerable<GastoDto>> GetCuotasAsync(Guid userId)
        {
            var cuotas = await _repo.GetCuotasAsync(userId);
            return _mapper.Map<IEnumerable<GastoDto>>(cuotas);
        }

        public async Task<IEnumerable<GastoDto>> CopiarRecurrentesAsync(int mes, int anio, Guid userId)
        {
            var mesPrevio  = mes == 1 ? 12 : mes - 1;
            var anioPrevio = mes == 1 ? anio - 1 : anio;
            var recurrentes = await _repo.GetRecurrentesAsync(mesPrevio, anioPrevio, userId);

            var creados = new List<GastoDto>();
            foreach (var r in recurrentes)
            {
                var nuevo = new Gasto
                {
                    Categoria    = r.Categoria,
                    Detalle      = r.Detalle,
                    Monto        = r.Monto,
                    Tipo         = r.Tipo,
                    Mes          = mes,
                    Anio         = anio,
                    EsRecurrente = true,
                    UserId       = userId
                };
                var creado = await _repo.CreateAsync(nuevo);
                creados.Add(_mapper.Map<GastoDto>(creado));
            }
            return creados;
        }
    }
}
