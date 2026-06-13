using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class PresupuestosService(IPresupuestosRepository repo, IMapper mapper)
    {
        private readonly IPresupuestosRepository _repo = repo;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<PresupuestoDto>> GetAllAsync(Guid userId)
        {
            var items = await _repo.GetAllAsync(userId);
            return _mapper.Map<IEnumerable<PresupuestoDto>>(items);
        }

        public async Task<PresupuestoDto> UpsertAsync(CrearPresupuestoDto dto, Guid userId)
        {
            var existente = await _repo.GetByCategoriaAsync(dto.Categoria, userId);
            if (existente is not null)
            {
                existente.Limite = dto.Limite;
                var updated = await _repo.UpdateAsync(existente);
                return _mapper.Map<PresupuestoDto>(updated);
            }

            var nuevo = _mapper.Map<Presupuesto>(dto);
            nuevo.UserId = userId;
            var creado = await _repo.CreateAsync(nuevo);
            return _mapper.Map<PresupuestoDto>(creado);
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
