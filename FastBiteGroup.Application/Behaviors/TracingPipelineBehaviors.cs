using FastBiteGroup.Contract.Abstractions.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FastBiteGroup.Application.Behaviors;

public sealed class TracingPipelineBehaviors<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;

    public TracingPipelineBehaviors(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        var elapsedTime = timer.ElapsedMilliseconds;
        var requestName = typeof(TRequest).Name;

        if (response is Result { IsFailure: true } result)
        {
            _logger.LogError("Request {RequestName} failed in {ElapsedTime} ms with error {@Error}",
                requestName, elapsedTime, result.Error);
        }
        else
        {
            _logger.LogInformation("Request {RequestName} executed successfully in {ElapsedTime} ms",
                requestName, elapsedTime);
        }

        return response;
    }
}
