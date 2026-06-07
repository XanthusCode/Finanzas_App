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
            var gasto = _mapper.Map<Gasto>(dto);
            gasto.UserId = userId;
            var creado = await _repo.CreateAsync(gasto);
            return _mapper.Map<GastoDto>(creado);
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
    }
}
