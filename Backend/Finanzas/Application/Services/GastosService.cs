using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class GastosService
    {
        private readonly IGastosRepository _repo;
        private readonly IMapper _mapper;

        public GastosService(IGastosRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GastoDto>> GetByMesAsync(int mes, int anio)
        {
            var gastos = await _repo.GetByMesAsync(mes, anio);
            return _mapper.Map<IEnumerable<GastoDto>>(gastos);
        }

        public async Task<GastoDto> CreateAsync(CrearGastoDto dto)
        {
            var gasto = _mapper.Map<Gasto>(dto);
            var creado = await _repo.CreateAsync(gasto);
            return _mapper.Map<GastoDto>(creado);
        }

        public async Task<GastoDto?> UpdateAsync(int id, CrearGastoDto dto)
        {
            var existente = await _repo.GetByIdAsync(id);
            if (existente is null) return null;

            _mapper.Map(dto, existente);
            var actualizado = await _repo.UpdateAsync(existente);
            return _mapper.Map<GastoDto>(actualizado);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existente = await _repo.GetByIdAsync(id);
            if (existente is null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }
    }
}
