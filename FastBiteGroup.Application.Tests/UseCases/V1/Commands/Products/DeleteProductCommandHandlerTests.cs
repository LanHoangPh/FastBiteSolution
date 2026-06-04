using FastBiteGroup.Application.UseCases.V1.Commands.Products;
using FastBiteGroup.Application.Tests.Common.Assertions;
using FastBiteGroup.Application.Tests.Common.Builders;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly ProductsRepository _productRepositoryMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _productRepositoryMock = Substitute.For<ProductsRepository>();
        _handler = new DeleteProductCommandHandler(_productRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidId_RemovesProduct()
    {
        // Arrange
        var command = ProductTestData.DeleteCommand();
        var existingProduct = ProductTestData.Product(name: "Old", description: "Old", price: 100);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeSuccess();
        _productRepositoryMock.Received(1).Remove(existingProduct);
    }

    [Fact]
    public async Task Handle_GivenNotFoundId_ReturnsFailure()
    {
        // Arrange
        var command = ProductTestData.DeleteCommand(99);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Throws(new KeyNotFoundException());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Product.NotFound");
        _productRepositoryMock.DidNotReceiveWithAnyArgs().Remove(default!);
    }
}
