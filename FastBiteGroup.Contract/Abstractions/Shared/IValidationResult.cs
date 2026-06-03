namespace FastBiteGroup.Contract.Abstractions.Shared;

public interface IValidationResult
{
    public static readonly Error ValidationError = new("ValidationError", "One or more validation errors occurred.");

    Error[] Errors { get; }
}
