using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class CategoriasService(ICategoriasRepository repo, IMapper mapper)
{
    private readonly ICategoriasRepository _repo = repo;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<CategoriaDto>> GetAllAsync(Guid userId)
    {
        var cats = await _repo.GetAllAsync(userId);
        return _mapper.Map<IEnumerable<CategoriaDto>>(cats);
    }

    public async Task<CategoriaDto> CreateAsync(CrearCategoriaDto dto, Guid userId)
    {
        var cat = _mapper.Map<Categoria>(dto);
        cat.UserId = userId;
        var creada = await _repo.CreateAsync(cat);
        return _mapper.Map<CategoriaDto>(creada);
    }

    public async Task<CategoriaDto?> UpdateAsync(Guid id, EditarCategoriaDto dto, Guid userId)
    {
        var existente = await _repo.GetByIdAsync(id, userId);
        if (existente is null) return null;

        _mapper.Map(dto, existente);
        var actualizada = await _repo.UpdateAsync(existente);
        return _mapper.Map<CategoriaDto>(actualizada);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var existente = await _repo.GetByIdAsync(id, userId);
        if (existente is null) return false;

        await _repo.DeleteAsync(id);
        return true;
    }
}
