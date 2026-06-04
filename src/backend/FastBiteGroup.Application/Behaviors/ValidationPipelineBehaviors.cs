using FastBiteGroup.Contract.Abstractions.Shared;
using FluentValidation;
using MediatR;

namespace FastBiteGroup.Application.Behaviors;

public sealed class ValidationPipelineBehaviors<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehaviors(IEnumerable<IValidator<TRequest>> validators)
    => _validators = validators;


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        Error[] errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Any())
        {
            return CreateValidationResult<TResponse>(errors);
        }

        return await next();

    }
    private static TResult CreateValidationResult<TResult>(Error[] errors)
    where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors) as TResult)!;
        }

        object validationResult = typeof(ValidationResult<>)
        .GetGenericTypeDefinition()
        .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
        .GetMethod(nameof(ValidationResult.WithErrors))!
        .Invoke(null, new object?[] { errors })!;

        return (TResult)validationResult;
    }
}
