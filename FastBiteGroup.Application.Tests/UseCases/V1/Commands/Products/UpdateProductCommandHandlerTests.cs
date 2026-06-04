using FastBiteGroup.Application.UseCases.V1.Commands.Products;
using FastBiteGroup.Application.Tests.Common.Assertions;
using FastBiteGroup.Application.Tests.Common.Builders;
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
        var command = ProductTestData.UpdateCommand();
        var existingProduct = ProductTestData.Product(name: "Old", description: "Old", price: 100);

        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeSuccess();
        existingProduct.Name.Should().Be("Updated Product");
        existingProduct.Price.Should().Be(150);
        _productRepositoryMock.Received(1).Update(existingProduct);
    }

    [Fact]
    public async Task Handle_GivenNotFoundId_ReturnsFailure()
    {
        // Arrange
        var command = ProductTestData.UpdateCommand(id: 99);

        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Throws(new KeyNotFoundException());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Product.NotFound");
    }

    [Fact]
    public async Task Handle_GivenNegativePrice_ReturnsFailure()
    {
        // Arrange
        var command = ProductTestData.UpdateCommand(price: -10);
        var existingProduct = ProductTestData.Product(name: "Old", description: "Old", price: 100);

        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Product.PriceInvalid");
        _productRepositoryMock.DidNotReceive().Update(existingProduct);
    }
}
