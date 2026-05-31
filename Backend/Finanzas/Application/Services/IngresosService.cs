using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class IngresosService
{
    private readonly IIngresosRepository _repo;
    private readonly IMapper _mapper;

    public IngresosService(IIngresosRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<IngresoDto>> GetByMesAsync(int mes, int anio)
    {
        var ingresos = await _repo.GetByMesAsync(mes, anio);
        return _mapper.Map<IEnumerable<IngresoDto>>(ingresos);
    }

    public async Task<IngresoDto> CreateAsync(CrearIngresoDto dto)
    {
        var ingreso = _mapper.Map<Ingreso>(dto);
        var creado = await _repo.CreateAsync(ingreso);
        return _mapper.Map<IngresoDto>(creado);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existente = await _repo.GetByIdAsync(id);
        if (existente is null) return false;

        await _repo.DeleteAsync(id);
        return true;
    }
}