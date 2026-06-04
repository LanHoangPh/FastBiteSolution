using MockQueryable;
using MockQueryable.NSubstitute;
using NSubstitute;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;
using ProductsRepository = FastBiteGroup.Domain.Abstractions.Repositories.IRepositoryBase<FastBiteGroup.Domain.Entities.Products, int>;

namespace FastBiteGroup.Application.Tests.Common;

internal static class RepositorySubstituteExtensions
{
    public static void ReturnsProducts(this ProductsRepository repository, IReadOnlyCollection<DomainProducts> products)
    {
        var productSet = products.ToList().BuildMockDbSet();
        repository.FindAll().Returns(productSet);
    }

    public static void ReturnsProductsForId(
        this ProductsRepository repository,
        IReadOnlyCollection<DomainProducts> products,
        int id)
    {
        repository.ReturnsProducts(products);

        var filteredSet = products
            .Where(product => product.Id == id)
            .ToList()
            .BuildMockDbSet();

        repository
            .FindAll(Arg.Any<System.Linq.Expressions.Expression<Func<DomainProducts, bool>>>())
            .Returns(filteredSet);
    }
}
