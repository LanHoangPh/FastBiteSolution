using FastBiteGroup.Contract.Abstractions.Shared;
using MediatR;

namespace FastBiteGroup.Contract.Abstractions.Message;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{ }
