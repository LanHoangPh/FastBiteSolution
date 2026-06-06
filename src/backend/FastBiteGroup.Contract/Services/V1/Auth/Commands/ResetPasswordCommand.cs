using FastBiteGroup.Contract.Abstractions.Message;

namespace FastBiteGroup.Contract.Services.V1.Auth.Commands;

public sealed record ResetPasswordCommand(
    string Email,
    string Otp,
    string NewPassword) : ICommand;
