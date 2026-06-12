namespace FastBiteGroup.Domain.Exceptions
{
    public static class ProductException
    {
        public class ProductNotFoundException(int id) : NotFoundException($"Product with id {id} not found.");
        public class ProductPriceInvalidException(decimal invalidPrice)
            : BadRequestException($"Price cannot be negative. Provided value: {invalidPrice}");

    }
}
