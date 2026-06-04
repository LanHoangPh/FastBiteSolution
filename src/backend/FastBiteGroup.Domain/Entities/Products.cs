using FastBiteGroup.Domain.Abstractions;
using static FastBiteGroup.Domain.Exceptions.ProductException;

namespace FastBiteGroup.Domain.Entities;

public class Products : EntityAuditBase<int>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }

    protected Products() { }

    public static Products Create(string name, string description, decimal price)
    {
        if (price < 0) throw new ProductPriceInvalidException(price);

        return new Products { Name = name, Description = description, Price = price };
    }

    public void Update(string name, string description, decimal price)
    {
        if (price < 0) throw new ProductPriceInvalidException(price);

        Name = name;
        Description = description;
        Price = price;
    }
}
