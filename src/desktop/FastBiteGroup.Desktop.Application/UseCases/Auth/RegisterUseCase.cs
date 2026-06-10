using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Domain.Models.Shared;

namespace FastBiteGroup.Desktop.Application.UseCases.Auth;

public sealed class RegisterUseCase : IUseCase<RegisterRequest, RegisterResponse>
{
    private readonly IAuthService _authService;

    public RegisterUseCase(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<Result<RegisterResponse>> ExecuteAsync(RegisterRequest request) =>
        _authService.RegisterAsync(request);
}
