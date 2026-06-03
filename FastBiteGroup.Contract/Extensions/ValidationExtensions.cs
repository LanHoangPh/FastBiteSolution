using FluentValidation;


namespace FastBiteGroup.Contract.Extensions;

/// <summary>
/// Reusable FluentValidation rule extensions shared across all validators in the Contract layer.
/// Each method follows the Clean Architecture constraint: no Infrastructure/Persistence dependencies.
/// </summary>
public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, Guid> NotEmptyGuid<T>(
        this IRuleBuilder<T, Guid> ruleBuilder,
        string errorCode)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode)
            .WithMessage($"'{typeof(T).Name}' must not contain an empty Guid.");
    }
    public static IRuleBuilderOptions<T, string> MustBeValidName<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string notEmptyErrorCode,
        string formatErrorCode)
    {
        return ruleBuilder
            .NotEmpty()
                .WithErrorCode(notEmptyErrorCode)
                .WithMessage("Name must not be empty.")
            .Matches(@"^[\p{L}\p{N}\s\-']+$")
                .WithErrorCode(formatErrorCode)
                .WithMessage("Name contains invalid characters. Only letters, digits, spaces, hyphens, and apostrophes are allowed.");
    }

    public static IRuleBuilderOptions<T, decimal> MustBePositiveNumber<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        string errorCode)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithErrorCode(errorCode)
            .WithMessage("Value must be greater than zero.");
    }
    public static IRuleBuilderOptions<T, string> MaxLength<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int max,
        string errorCode)
    {
        return ruleBuilder
            .MaximumLength(max)
            .WithErrorCode(errorCode)
            .WithMessage($"Value must not exceed {max} characters.");
    }
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string notEmptyErrorCode,
        string formatErrorCode)
    {
        return ruleBuilder
            .NotEmpty()
                .WithErrorCode(notEmptyErrorCode)
                .WithMessage("Email must not be empty.")
            .EmailAddress()
                .WithErrorCode(formatErrorCode)
                .WithMessage("Email is not in a valid format.");
    }
}
