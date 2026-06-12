using System.Diagnostics;

namespace FastBiteGroup.Application.Behaviors;

public sealed class PerformancePipelineBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        timer.Stop();
        var elapsedMilliseconds = timer.ElapsedMilliseconds;
        if (elapsedMilliseconds <= 5000)
        {
            return response;
        }
        var requestName = typeof(TRequest).Name;
        logger.LogWarning(
            "FastBiteGroup long running request: {RequestName} ({ElapsedMilliseconds} milliseconds)",
            requestName,
            elapsedMilliseconds);

        return response;
    }
}
