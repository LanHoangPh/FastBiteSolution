using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Domain.Models.Shared;

namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface IAuthClient
{
    // [Post("/api/v1/auth/login")]
    // Task<Result<object>> LoginAsync([Body] object request);

    Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}
