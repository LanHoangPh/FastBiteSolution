namespace FastBiteGroup.Domain.Exceptions;

public class BadRequestException : DomainException
{
    protected BadRequestException(string message) : base("Bad Resquest", message)
    {
    }
}
