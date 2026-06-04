using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static FastBiteGroup.Domain.Exceptions.ProductException;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Products;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly ProductsRepository _productRepository;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        ProductsRepository productRepository,
        ILogger<UpdateProductCommandHandler>? logger = null)
    {
        _productRepository = productRepository;
        _logger = logger ?? NullLogger<UpdateProductCommandHandler>.Instance;
    }

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
            _logger.LogWarning("Product update failed: product not found. ProductId: {ProductId}", request.Id);
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
        _logger.LogInformation("Product updated. ProductId: {ProductId}", request.Id);

        return Result.Success();
    }
}
