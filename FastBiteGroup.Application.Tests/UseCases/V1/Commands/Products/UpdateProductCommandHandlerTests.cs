using FastBiteGroup.Application.UseCases.V1.Commands.Products;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly ProductsRepository _productRepositoryMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productRepositoryMock = Substitute.For<ProductsRepository>();
        _handler = new UpdateProductCommandHandler(_productRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_UpdatesProduct()
    {
        // Arrange
        var command = new UpdateProductCommand(1, "Updated Product", "Updated Description", 150);
        var existingProduct = DomainProducts.Create("Old", "Old", 100);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.Name.Should().Be("Updated Product");
        existingProduct.Price.Should().Be(150);
        _productRepositoryMock.Received(1).Update(existingProduct);
    }

    [Fact]
    public async Task Handle_GivenNotFoundId_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateProductCommand(99, "Updated Product", "Updated Description", 150);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Throws(new KeyNotFoundException());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }

    [Fact]
    public async Task Handle_GivenNegativePrice_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateProductCommand(1, "Updated Product", "Updated Description", -10);
        var existingProduct = DomainProducts.Create("Old", "Old", 100);
        
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.PriceInvalid");
        _productRepositoryMock.DidNotReceive().Update(existingProduct);
    }
}
