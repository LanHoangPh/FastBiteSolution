using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FastBiteGroup.Application.Behaviors;

public sealed class PerformancePipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public PerformancePipelineBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();
        var elapsedMilliseconds = timer.ElapsedMilliseconds;
        if (elapsedMilliseconds <= 5000)
        {
            return response;
        }
        var requestName = typeof(TRequest).Name;
        _logger.LogWarning("FastBiteGroup Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) {@Request}",
            requestName, elapsedMilliseconds, request);
        return response;
    }
}
