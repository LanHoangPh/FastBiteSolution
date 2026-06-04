using FastBiteGroup.Contract.Services.V1.Product.Commands;
using DomainProducts = FastBiteGroup.Domain.Entities.Products;

namespace FastBiteGroup.Application.Tests.Common.Builders;

internal static class ProductTestData
{
    public static DomainProducts Product(
        int id = 1,
        string name = "Product 1",
        string description = "Description 1",
        decimal price = 100m,
        DateTimeOffset? createdAt = null)
    {
        var product = DomainProducts.Create(name, description, price);
        product.Id = id;
        product.CreatedAt = createdAt ?? DateTimeOffset.UtcNow;

        return product;
    }

    public static CreateProductCommand CreateCommand(
        string name = "Product 1",
        string description = "Description 1",
        decimal price = 100m)
        => new(name, description, price);

    public static UpdateProductCommand UpdateCommand(
        int id = 1,
        string name = "Updated Product",
        string description = "Updated Description",
        decimal price = 150m)
        => new(id, name, description, price);

    public static DeleteProductCommand DeleteCommand(int id = 1)
        => new(id);
}
