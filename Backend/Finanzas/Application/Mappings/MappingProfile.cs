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

        // Categoria
        CreateMap<Categoria, CategoriaDto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString()));
        CreateMap<CrearCategoriaDto, Categoria>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => Enum.Parse<TipoGasto>(s.Tipo)));
    }
}