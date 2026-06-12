namespace FastBiteGroup.Application.Behaviors;

public sealed class ValidationPipelineBehaviors<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Any())
        {
            return CreateValidationResult<TResponse>(errors);
        }

        return await next(cancellationToken);

    }
    private static TResult CreateValidationResult<TResult>(Error[] errors)
    where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors) as TResult)!;
        }

        var validationResult = typeof(ValidationResult<>)
        .GetGenericTypeDefinition()
        .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
        .GetMethod(nameof(ValidationResult.WithErrors))!
        .Invoke(null, [errors])!;

        return (TResult)validationResult;
    }
}
