using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Contract.Abstractions.Message;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{ }

// Handler cho Command có trả về dữ liệu (TResponse)
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{ }
