using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class CategoriasService
{
    private readonly ICategoriasRepository _repo;
    private readonly IMapper _mapper;

    public CategoriasService(ICategoriasRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoriaDto>> GetAllAsync()
    {
        var cats = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoriaDto>>(cats);
    }

    public async Task<CategoriaDto> CreateAsync(CrearCategoriaDto dto)
    {
        var cat = _mapper.Map<Categoria>(dto);
        var creada = await _repo.CreateAsync(cat);
        return _mapper.Map<CategoriaDto>(creada);
    }

    public async Task<CategoriaDto?> UpdateAsync(int id, CrearCategoriaDto dto)
    {
        var existente = await _repo.GetByIdAsync(id);
        if (existente is null) return null;

        _mapper.Map(dto, existente);
        var actualizada = await _repo.UpdateAsync(existente);
        return _mapper.Map<CategoriaDto>(actualizada);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existente = await _repo.GetByIdAsync(id);
        if (existente is null) return false;

        await _repo.DeleteAsync(id);
        return true;
    }
}