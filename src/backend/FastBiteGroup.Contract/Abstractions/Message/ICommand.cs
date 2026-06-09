using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Contract.Abstractions.Message;

public interface ICommand : IRequest<Result>
{
}
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
