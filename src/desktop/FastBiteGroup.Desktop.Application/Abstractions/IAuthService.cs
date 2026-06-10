using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Domain.Models.Shared;

namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> TryAutoLoginAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
