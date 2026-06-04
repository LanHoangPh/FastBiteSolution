using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Products;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly ProductsRepository _productRepository;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        ProductsRepository productRepository,
        ILogger<DeleteProductCommandHandler>? logger = null)
    {
        _productRepository = productRepository;
        _logger = logger ?? NullLogger<DeleteProductCommandHandler>.Instance;
    }

    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        FastBiteGroup.Domain.Entities.Products product;
        try
        {
            product = await _productRepository.FindByIdAsync(request.Id, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Product delete failed: product not found. ProductId: {ProductId}", request.Id);
            return Result.Failure(
                new Error("Product.NotFound", $"Product with id {request.Id} not found."));
        }

        _productRepository.Remove(product);
        _logger.LogInformation("Product deleted. ProductId: {ProductId}", request.Id);

        return Result.Success();
    }
}
