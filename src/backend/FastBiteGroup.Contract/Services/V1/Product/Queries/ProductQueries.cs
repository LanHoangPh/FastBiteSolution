using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Responses;

namespace FastBiteGroup.Contract.Services.V1.Product.Queries;

public static class ProductQueries
{
    public sealed record GetAllProductsQuery : IQuery<PagedResult<ProductResponse>>;

    public sealed record GetProductByIdQuery(int Id) : IQuery<ProductResponse>;
}
