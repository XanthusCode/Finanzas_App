using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class CategoriasService(ICategoriasRepository repo, IGastosRepository gastosRepo, IMapper mapper)
{
    public async Task<IEnumerable<CategoriaDto>> GetAllAsync(Guid userId)
    {
        var cats = await repo.GetAllAsync(userId);
        return mapper.Map<IEnumerable<CategoriaDto>>(cats);
    }

    public async Task<CategoriaDto> CreateAsync(CrearCategoriaDto dto, Guid userId)
    {
        var cat = mapper.Map<Categoria>(dto);
        cat.UserId = userId;
        var creada = await repo.CreateAsync(cat);
        return mapper.Map<CategoriaDto>(creada);
    }

    public async Task<CategoriaDto?> UpdateAsync(Guid id, EditarCategoriaDto dto, Guid userId)
    {
        var existente = await repo.GetByIdAsync(id, userId);
        if (existente is null) return null;

        mapper.Map(dto, existente);
        var actualizada = await repo.UpdateAsync(existente);
        return mapper.Map<CategoriaDto>(actualizada);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var existente = await repo.GetByIdAsync(id, userId);
        if (existente is null) return false;

        if (await gastosRepo.ExistePorCategoriaAsync(existente.Nombre, userId))
            throw new InvalidOperationException("La categoría está en uso por uno o más gastos.");

        await repo.DeleteAsync(id);
        return true;
    }
}
