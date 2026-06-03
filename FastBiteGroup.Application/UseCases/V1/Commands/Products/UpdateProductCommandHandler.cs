using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using static FastBiteGroup.Domain.Exceptions.ProductException;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Products;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly ProductsRepository _productRepository;

    public UpdateProductCommandHandler(ProductsRepository productRepository)
        => _productRepository = productRepository;

    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        FastBiteGroup.Domain.Entities.Products product;
        try
        {
            product = await _productRepository.FindByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(
                new Error("Product.NotFound", $"Product with id {request.Id} not found."));
        }

        try
        {
            product.Update(request.Name, request.Description, request.Price);
        }
        catch (ProductPriceInvalidException ex)
        {
            return Result.Failure(new Error("Product.PriceInvalid", ex.Message));
        }

        _productRepository.Update(product);
        return Result.Success();
    }
}
