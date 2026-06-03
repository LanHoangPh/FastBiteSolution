using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using static FastBiteGroup.Domain.Exceptions.ProductException;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Products;

internal sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, int>
{
    private readonly ProductsRepository _productRepository;

    public CreateProductCommandHandler(ProductsRepository productRepository)
        => _productRepository = productRepository;

    public Task<Result<int>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = DomainProducts.Create(request.Name, request.Description, request.Price);
            _productRepository.Add(product);
            return Task.FromResult(Result.Success(product.Id));
        }
        catch (ProductPriceInvalidException ex)
        {
            return Task.FromResult(
                Result.Failure<int>(new Error("Product.PriceInvalid", ex.Message)));
        }
    }
}
