using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Queries;
using FastBiteGroup.Contract.Services.V1.Product.Responses;
using Microsoft.EntityFrameworkCore;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Queries.Products;

internal sealed class GetAllProductsQueryHandler
    : IQueryHandler<ProductQueries.GetAllProductsQuery, PagedResult<ProductResponse>>
{
    private readonly ProductsRepository _productRepository;

    public GetAllProductsQueryHandler(ProductsRepository productRepository)
        => _productRepository = productRepository;

    public async Task<Result<PagedResult<ProductResponse>>> Handle(
        ProductQueries.GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _productRepository.FindAll();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ProductResponse>(
            items,
            pageIndex: PagedResult<ProductResponse>.DefaultPageIndex,
            pageSize: totalCount,
            totalCount: totalCount);

        return Result.Success(pagedResult);
    }
}
