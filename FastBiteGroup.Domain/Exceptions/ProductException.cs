namespace FastBiteGroup.Domain.Exceptions
{
    public static class ProductException
    {
        public class ProductNotFoundException : NotFoundException
        {
            public ProductNotFoundException(int id)
                : base($"Product with id {id} not found.")
            {
            }
        }
        public class ProductPriceInvalidException : BadRequestException
        {
            public ProductPriceInvalidException(decimal invalidPrice)
                : base($"Price cannot be negative. Provided value: {invalidPrice}") { }
        }

    }
}
