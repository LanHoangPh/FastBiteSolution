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

public class GetAllProductsQueryHandlerTests
{
    private readonly ProductsRepository _productRepositoryMock;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _productRepositoryMock = Substitute.For<ProductsRepository>();
        _handler = new GetAllProductsQueryHandler(_productRepositoryMock);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResultOfProducts()
    {
        // Arrange
        var command = new ProductQueries.GetAllProductsQuery();
        var older = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newer = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var products = new[]
        {
            ProductTestData.Product(id: 1, name: "Product 1", description: "Desc 1", price: 100, createdAt: older),
            ProductTestData.Product(id: 2, name: "Product 2", description: "Desc 2", price: 200, createdAt: newer)
        };
        _productRepositoryMock.ReturnsProducts(products);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var page = result.ShouldBeSuccess();
        page.Items.Should().HaveCount(2);
        page.TotalCount.Should().Be(2);
        page.Items.Select(product => product.Id).Should().Equal(2, 1);
    }
}
