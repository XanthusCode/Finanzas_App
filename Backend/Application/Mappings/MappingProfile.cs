using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Gasto
        CreateMap<Gasto, GastoDto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString()));
        CreateMap<CrearGastoDto, Gasto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => Enum.Parse<TipoGasto>(s.Tipo)));

        // Ingreso
        CreateMap<Ingreso, IngresoDto>()
            .ForMember(d => d.Frecuencia, o => o.MapFrom(s => s.Frecuencia == null ? null : s.Frecuencia.Value.ToString()));
        CreateMap<CrearIngresoDto, Ingreso>()
            .ForMember(d => d.Frecuencia, o => o.MapFrom(s => ParseFrecuencia(s.Frecuencia)));
        CreateMap<EditarIngresoDto, Ingreso>()
            .ForMember(d => d.Id,     o => o.Ignore())
            .ForMember(d => d.Mes,    o => o.Ignore())
            .ForMember(d => d.Anio,   o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Frecuencia, o => o.MapFrom(s => ParseFrecuencia(s.Frecuencia)));

        // Categoria
        CreateMap<Categoria, CategoriaDto>();
        CreateMap<CrearCategoriaDto, Categoria>();
        CreateMap<EditarCategoriaDto, Categoria>()
            .ForMember(d => d.Id,     o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Activa, o => o.Ignore());

        // Presupuesto
        CreateMap<Presupuesto, PresupuestoDto>();
        CreateMap<CrearPresupuestoDto, Presupuesto>();

        // Meta
        CreateMap<Meta, MetaDto>();
        CreateMap<CrearMetaDto, Meta>();

        // Deuda
        CreateMap<Deuda, DeudaDto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString()));
        CreateMap<CrearDeudaDto, Deuda>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => Enum.Parse<TipoDeuda>(s.Tipo)));

        // Abono
        CreateMap<Abono, AbonoDto>();

        // Cuenta
        CreateMap<Cuenta, CuentaDto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString()));
        CreateMap<CrearCuentaDto, Cuenta>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => Enum.Parse<TipoCuenta>(s.Tipo)));

        // MovimientoCuenta
        CreateMap<MovimientoCuenta, MovimientoCuentaDto>();

        // MovimientoMeta
        CreateMap<MovimientoMeta, MovimientoMetaDto>();
    }

    private static FrecuenciaIngreso? ParseFrecuencia(string? frecuencia) =>
        string.IsNullOrEmpty(frecuencia) ? null : Enum.Parse<FrecuenciaIngreso>(frecuencia);
}