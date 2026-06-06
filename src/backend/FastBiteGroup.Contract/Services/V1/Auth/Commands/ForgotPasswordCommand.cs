using FastBiteGroup.Contract.Abstractions.Message;

namespace FastBiteGroup.Contract.Services.V1.Auth.Commands;

public sealed record ForgotPasswordCommand(
    string Email) : ICommand;
