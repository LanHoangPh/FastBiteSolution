using FastBiteGroup.Application.UseCases.V1.Commands.Products;
using FastBiteGroup.Application.Tests.Common.Assertions;
using FastBiteGroup.Application.Tests.Common.Builders;
using FluentAssertions;
using NSubstitute;
using Xunit;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Commands.Products;

public class CreateProductCommandHandlerTests
{
    private readonly ProductsRepository _productRepositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = Substitute.For<ProductsRepository>();
        _handler = new CreateProductCommandHandler(_productRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_ReturnsProductId()
    {
        // Arrange
        var command = ProductTestData.CreateCommand(name: "Product 1", description: "Description", price: 100);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeSuccess();
        _productRepositoryMock.Received(1).Add(Arg.Is<DomainProducts>(p => p.Name == command.Name && p.Price == command.Price));
    }

    [Fact]
    public async Task Handle_GivenNegativePrice_ReturnsFailure()
    {
        // Arrange
        var command = ProductTestData.CreateCommand(price: -10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Product.PriceInvalid");
        _productRepositoryMock.DidNotReceiveWithAnyArgs().Add(default!);
    }
}
