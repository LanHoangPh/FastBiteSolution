using AutoMapper;
using FastBiteGroup.Contract.Services.V1.Product.Responses;
using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Application.Mappers;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<Products, ProductResponse>()
            .ConstructUsing(src => new ProductResponse(
                src.Id,
                src.Name,
                src.Description,
                src.Price,
                src.CreatedAt,
                src.UpdatedAt));
    }
}
