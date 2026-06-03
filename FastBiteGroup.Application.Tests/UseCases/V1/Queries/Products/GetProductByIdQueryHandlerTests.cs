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
        var products = new List<DomainProducts>
        {
            DomainProducts.Create("Product 1", "Desc 1", 100)
        };
        // To mock FindById, we actually just need to mock FindByIdAsync if it uses that,
        // Wait, GetProductByIdQueryHandler uses FindByIdAsync() directly from repo, let's verify if it uses IQueryable or FindByIdAsync.
        // I will assume it uses FindByIdAsync or FindSingleAsync. Let's mock both just in case, or look at the code.
        
        // Let's assume it returns a product when FindByIdAsync is called, or we can mock FindAll.
        _productRepositoryMock.FindByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(products[0]);
            
        // If it uses FindAll + FirstOrDefaultAsync
        var mockDbSet = products.BuildMockDbSet();
        _productRepositoryMock.FindAll(Arg.Any<System.Linq.Expressions.Expression<Func<DomainProducts, bool>>>()).Returns(mockDbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ReturnsFailure()
    {
        // Arrange
        var command = new ProductQueries.GetProductByIdQuery(99);
        var products = new List<DomainProducts>();
        
        var mockDbSet = products.BuildMockDbSet();
        _productRepositoryMock.FindAll(Arg.Any<System.Linq.Expressions.Expression<Func<DomainProducts, bool>>>()).Returns(mockDbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}
