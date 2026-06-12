using System.Diagnostics;

namespace FastBiteGroup.Application.Behaviors;

public sealed class TracingPipelineBehaviors<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        timer.Stop();

        var elapsedTime = timer.ElapsedMilliseconds;
        var requestName = typeof(TRequest).Name;

        if (response is Result { IsFailure: true } result)
        {
            logger.LogError("Request {RequestName} failed in {ElapsedTime} ms with error {@Error}",
                requestName, elapsedTime, result.Error);
        }
        else
        {
            logger.LogInformation("Request {RequestName} executed successfully in {ElapsedTime} ms",
                requestName, elapsedTime);
        }

        return response;
    }
}
