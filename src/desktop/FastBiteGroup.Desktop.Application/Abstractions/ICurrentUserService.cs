using FastBiteGroup.Desktop.Application.Models.Auth;

namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    UserDto? User { get; }

    void SetUser(UserDto user);
    Task LoadCurrentUserAsync(CancellationToken cancellationToken = default);
    void Clear();
}
