using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services;

public class IngresosService(IIngresosRepository repo, IMapper mapper, ICuentaRepository cuentaRepo)
{
    private readonly IIngresosRepository _repo = repo;
    private readonly IMapper _mapper = mapper;

    // El CuentaId viene del cliente: solo se acepta si la cuenta es del usuario
    private async Task<Guid?> CuentaPropiaAsync(Guid? cuentaId, Guid userId) =>
        cuentaId is Guid id && await cuentaRepo.GetByIdAsync(id, userId) is not null ? id : null;

    public async Task<IEnumerable<IngresoDto>> GetByMesAsync(int mes, int anio, Guid userId)
    {
        var ingresos = await _repo.GetByMesAsync(mes, anio, userId);
        return _mapper.Map<IEnumerable<IngresoDto>>(ingresos);
    }

    public async Task<IngresoDto> CreateAsync(CrearIngresoDto dto, Guid userId)
    {
        dto.CuentaId = await CuentaPropiaAsync(dto.CuentaId, userId);
        var ingreso = _mapper.Map<Ingreso>(dto);
        ingreso.UserId = userId;
        var creado = await _repo.CreateAsync(ingreso);
        return _mapper.Map<IngresoDto>(creado);
    }

    public async Task<IngresoDto?> UpdateAsync(Guid id, EditarIngresoDto dto, Guid userId)
    {
        var existente = await _repo.GetByIdAsync(id, userId);
        if (existente is null) return null;

        dto.CuentaId = await CuentaPropiaAsync(dto.CuentaId, userId);
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
        var delMes = (await _repo.GetByMesAsync(mes, anio, userId)).ToList();

        foreach (var r in recurrentes)
        {
            if (delMes.Any(i => i.EsRecurrente && i.Concepto == r.Concepto))
                continue;

            var nuevo = new Ingreso
            {
                Concepto     = r.Concepto,
                Monto        = r.Monto,
                Mes          = mes,
                Anio         = anio,
                EsRecurrente = true,
                UserId       = userId,
                CuentaId     = r.CuentaId,
                Frecuencia   = r.Frecuencia,
                DiaPago      = r.DiaPago
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
