using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class DeudaService(IDeudaRepository repo, IMapper mapper)
    {
        private readonly IDeudaRepository _repo   = repo;
        private readonly IMapper          _mapper = mapper;

        public async Task<IEnumerable<DeudaDto>> GetAllAsync(Guid userId)
        {
            var items = await _repo.GetAllAsync(userId);
            return _mapper.Map<IEnumerable<DeudaDto>>(items);
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
            deuda.Pagada = !deuda.Pagada;
            var actualizada = await _repo.UpdateAsync(deuda);
            return _mapper.Map<DeudaDto>(actualizada);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var deuda = await _repo.GetByIdAsync(id, userId);
            if (deuda is null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }
    }
}
