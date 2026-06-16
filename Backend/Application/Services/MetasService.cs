using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class MetasService(IMetasRepository repo, IMapper mapper)
    {
        private readonly IMetasRepository _repo = repo;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<MetaDto>> GetAllAsync(Guid userId)
        {
            var items = await _repo.GetAllAsync(userId);
            return _mapper.Map<IEnumerable<MetaDto>>(items);
        }

        public async Task<MetaDto> CreateAsync(CrearMetaDto dto, Guid userId)
        {
            var meta = _mapper.Map<Meta>(dto);
            meta.UserId = userId;
            var creada = await _repo.CreateAsync(meta);
            return _mapper.Map<MetaDto>(creada);
        }

        public async Task<MetaDto?> EditarAsync(Guid id, EditarMetaDto dto, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return null;

            meta.Nombre        = dto.Nombre;
            meta.MontoObjetivo = dto.MontoObjetivo;
            meta.FechaLimite   = dto.FechaLimite;
            meta.Completada    = meta.MontoActual >= dto.MontoObjetivo;
            var actualizada = await _repo.UpdateAsync(meta);
            return _mapper.Map<MetaDto>(actualizada);
        }

        public async Task<MetaDto?> AbonarAsync(Guid id, AbonarMetaDto dto, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return null;

            meta.MontoActual = Math.Min(meta.MontoActual + dto.Monto, meta.MontoObjetivo);
            meta.Completada = meta.MontoActual >= meta.MontoObjetivo;
            var actualizada = await _repo.UpdateAsync(meta);
            return _mapper.Map<MetaDto>(actualizada);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }
    }
}
