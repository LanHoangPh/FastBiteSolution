using FastBiteGroup.Application.UseCases.V1.Commands.Products;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;
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
        var command = new DeleteProductCommand(1);
        var existingProduct = DomainProducts.Create("Old", "Old", 100);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepositoryMock.Received(1).Remove(existingProduct);
    }

    [Fact]
    public async Task Handle_GivenNotFoundId_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteProductCommand(99);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Throws(new KeyNotFoundException());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
        _productRepositoryMock.DidNotReceiveWithAnyArgs().Remove(default!);
    }
}
