using FastBiteGroup.Desktop.Application.Models.Auth;
using Refit;

namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface IUserClient
{
    [Get("/api/v1/user/me")]
    Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
