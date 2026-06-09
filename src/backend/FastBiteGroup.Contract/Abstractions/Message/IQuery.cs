using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Contract.Abstractions.Message;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
