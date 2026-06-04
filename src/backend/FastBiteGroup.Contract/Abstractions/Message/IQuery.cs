using FastBiteGroup.Contract.Abstractions.Shared;
using MediatR;

namespace FastBiteGroup.Contract.Abstractions.Message;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
