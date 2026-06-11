using FastBiteGroup.Desktop.Application.Models.Auth;
using Refit;

namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface IAuthClient
{
    [Post("/api/v1/auth/register")]
    Task<RegisterResponse> RegisterAsync([Body] RegisterRequest request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/refresh")]
    Task<AuthResponse> RefreshAsync([Body] RefreshTokenRequest request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/logout")]
    Task LogoutAsync([Body] LogoutRequest request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/forgot-password")]
    Task ForgotPasswordAsync([Body] ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    [Post("/api/v1/auth/reset-password")]
    Task ResetPasswordAsync([Body] ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
