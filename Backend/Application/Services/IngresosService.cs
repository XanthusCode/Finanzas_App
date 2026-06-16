using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class IngresosService(IIngresosRepository repo, IMapper mapper)
{
    private readonly IIngresosRepository _repo = repo;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<IngresoDto>> GetByMesAsync(int mes, int anio, Guid userId)
    {
        var ingresos = await _repo.GetByMesAsync(mes, anio, userId);
        return _mapper.Map<IEnumerable<IngresoDto>>(ingresos);
    }

    public async Task<IngresoDto> CreateAsync(CrearIngresoDto dto, Guid userId)
    {
        var ingreso = _mapper.Map<Ingreso>(dto);
        ingreso.UserId = userId;
        var creado = await _repo.CreateAsync(ingreso);
        return _mapper.Map<IngresoDto>(creado);
    }

    public async Task<IngresoDto?> UpdateAsync(Guid id, EditarIngresoDto dto, Guid userId)
    {
        var existente = await _repo.GetByIdAsync(id, userId);
        if (existente is null) return null;

        _mapper.Map(dto, existente);
        var actualizada = await _repo.UpdateAsync(existente);
        return _mapper.Map<IngresoDto>(actualizada);
    }

    public async Task<IEnumerable<IngresoDto>> CopiarRecurrentesAsync(int mes, int anio, Guid userId)
    {
        var mesPrevio  = mes == 1 ? 12 : mes - 1;
        var anioPrevio = mes == 1 ? anio - 1 : anio;
        var recurrentes = await _repo.GetRecurrentesAsync(mesPrevio, anioPrevio, userId);

        var creados = new List<IngresoDto>();
        foreach (var r in recurrentes)
        {
            var nuevo = new Ingreso
            {
                Concepto     = r.Concepto,
                Monto        = r.Monto,
                Mes          = mes,
                Anio         = anio,
                EsRecurrente = true,
                UserId       = userId
            };
            var creado = await _repo.CreateAsync(nuevo);
            creados.Add(_mapper.Map<IngresoDto>(creado));
        }
        return creados;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var existente = await _repo.GetByIdAsync(id, userId);
        if (existente is null) return false;

        await _repo.DeleteAsync(id);
        return true;
    }
}
