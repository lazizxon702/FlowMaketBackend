using AutoMapper;
using FlowMarket.Application.Contracts;
using FlowMarket.Domain.Entities;

namespace FlowMarket.Application.Common;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
