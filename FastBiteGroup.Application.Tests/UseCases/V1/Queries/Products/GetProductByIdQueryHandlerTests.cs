using FastBiteGroup.Application.UseCases.V1.Queries.Products;
using FastBiteGroup.Application.Tests.Common;
using FastBiteGroup.Application.Tests.Common.Assertions;
using FastBiteGroup.Application.Tests.Common.Builders;
using FastBiteGroup.Contract.Services.V1.Product.Queries;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.Tests.UseCases.V1.Queries.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly ProductsRepository _productRepositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _productRepositoryMock = Substitute.For<ProductsRepository>();
        _handler = new GetProductByIdQueryHandler(_productRepositoryMock);
    }

    [Fact]
    public async Task Handle_GivenValidId_ReturnsProduct()
    {
        // Arrange
        var command = new ProductQueries.GetProductByIdQuery(1);
        var products = new[]
        {
            ProductTestData.Product(id: 1, name: "Product 1", description: "Desc 1", price: 100),
            ProductTestData.Product(id: 2, name: "Product 2", description: "Desc 2", price: 200)
        };
        _productRepositoryMock.ReturnsProductsForId(products, command.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var product = result.ShouldBeSuccess();
        product.Id.Should().Be(1);
        product.Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ReturnsFailure()
    {
        // Arrange
        var command = new ProductQueries.GetProductByIdQuery(99);
        var products = new[]
        {
            ProductTestData.Product(id: 1),
            ProductTestData.Product(id: 2)
        };
        _productRepositoryMock.ReturnsProductsForId(products, command.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith("Product.NotFound");
    }
}
