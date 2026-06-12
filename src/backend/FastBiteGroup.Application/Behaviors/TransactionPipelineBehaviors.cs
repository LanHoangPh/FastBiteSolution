using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Application.Behaviors
{
    public class TransactionPipelineBehaviors<TRequest, TResponse>(IUnitOfWork unitOfWork)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Result
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return await unitOfWork.ExecuteTransactionAsync(
                async () => await next(cancellationToken),
                cancellationToken);
        }
    }
}
