using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Queries;
using FastBiteGroup.Contract.Services.V1.Product.Responses;
using Microsoft.EntityFrameworkCore;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Products;

internal sealed class GetProductByIdQueryHandler
    : IQueryHandler<ProductQueries.GetProductByIdQuery, ProductResponse>
{
    private readonly ProductsRepository _productRepository;

    public GetProductByIdQueryHandler(ProductsRepository productRepository)
        => _productRepository = productRepository;

    public async Task<Result<ProductResponse>> Handle(
        ProductQueries.GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository
            .FindAll(p => p.Id == request.Id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return Result.Failure<ProductResponse>(
                new Error("Product.NotFound", $"Product with id {request.Id} not found."));

        return Result.Success(product);
    }
}
