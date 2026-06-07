namespace FastBiteGroup.Desktop.Domain.Models.Shared;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "An unknown error occurred.");

    public static implicit operator string(Error error) => error.Code;
}
