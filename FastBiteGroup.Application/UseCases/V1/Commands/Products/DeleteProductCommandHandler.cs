using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Products;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly ProductsRepository _productRepository;

    public DeleteProductCommandHandler(ProductsRepository productRepository)
        => _productRepository = productRepository;

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
            return Result.Failure(
                new Error("Product.NotFound", $"Product with id {request.Id} not found."));
        }

        _productRepository.Remove(product);
        return Result.Success();
    }
}
