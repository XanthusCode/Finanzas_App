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
        CreateMap<Ingreso, IngresoDto>();
        CreateMap<CrearIngresoDto, Ingreso>();
        CreateMap<EditarIngresoDto, Ingreso>()
            .ForMember(d => d.Id,     o => o.Ignore())
            .ForMember(d => d.Mes,    o => o.Ignore())
            .ForMember(d => d.Anio,   o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore());

        // Categoria
        CreateMap<Categoria, CategoriaDto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString()));
        CreateMap<CrearCategoriaDto, Categoria>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => Enum.Parse<TipoGasto>(s.Tipo)));
        CreateMap<EditarCategoriaDto, Categoria>()
            .ForMember(d => d.Id,     o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Activa, o => o.Ignore())
            .ForMember(d => d.Tipo,   o => o.MapFrom(s => Enum.Parse<TipoGasto>(s.Tipo)));

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
    }
}