using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Application.Models.Auth;

namespace FastBiteGroup.Desktop.Infrastructure.Authentication;

public sealed class CurrentUserService : ICurrentUserService
{
    private UserDto? _user;
    private readonly IUserClient _userClient;

    public CurrentUserService(IUserClient userClient)
    {
        _userClient = userClient;
    }

    public bool IsAuthenticated => _user != null;
    
    public UserDto? User => _user;

    public void SetUser(UserDto user)
    {
        _user = user;
    }

    public async Task LoadCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userDto = await _userClient.GetCurrentUserAsync(cancellationToken);
            _user = userDto;
        }
        catch
        {
            // If fetching fails (e.g. 401 or network), we don't crash but user remains null.
            _user = null;
        }
    }

    public void Clear()
    {
        _user = null;
    }
}
