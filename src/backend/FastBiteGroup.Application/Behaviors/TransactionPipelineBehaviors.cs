using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Domain.Abstractions;
using MediatR;

namespace FastBiteGroup.Application.Behaviors
{
    public class TransactionPipelineBehaviors<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
     where TResponse : Result
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionPipelineBehaviors(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ExecuteTransactionAsync(
                async () => await next(),
                cancellationToken);
        }
    }
}
