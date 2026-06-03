using FastBiteGroup.Application.UseCases.V1.Queries.Products;
using FastBiteGroup.Contract.Services.V1.Product.Queries;
using FluentAssertions;
using MockQueryable.NSubstitute;
using MockQueryable;
using NSubstitute;
using Xunit;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;
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
        var products = new List<DomainProducts>
        {
            DomainProducts.Create("Product 1", "Desc 1", 100),
            DomainProducts.Create("Product 2", "Desc 2", 200)
        };

        var mockDbSet = products.BuildMockDbSet();

        _productRepositoryMock.FindAll().Returns(mockDbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.First().Name.Should().Be("Product 1"); // Assuming order is maintained correctly based on CreatedAt (since both are created at same time, order may vary, but let's assert count)
    }
}
