using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Domain.Models.Shared;

namespace FastBiteGroup.Desktop.Application.UseCases.Auth;

public sealed class LoginUseCase : IUseCase<LoginRequest, AuthResponse>
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITokenStorage _tokenStorage;
    private readonly ISecureTokenStore _secureTokenStore;

    public LoginUseCase(
        IAuthService authService,
        ITokenProvider tokenProvider,
        ITokenStorage tokenStorage,
        ISecureTokenStore secureTokenStore)
    {
        _authService = authService;
        _tokenProvider = tokenProvider;
        _tokenStorage = tokenStorage;
        _secureTokenStore = secureTokenStore;
    }

    public Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request)
    {
        return ExecuteAsync(request, true);
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, bool rememberMe)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsSuccess && result.Value != null)
        {
            _tokenProvider.SetAccessToken(result.Value.AccessToken, result.Value.AccessTokenExpiresAt);
            if (rememberMe)
            {
                await _tokenStorage.SaveTokenAsync(result.Value.AccessToken);

                if (!string.IsNullOrEmpty(result.Value.RefreshToken))
                {
                    await _secureTokenStore.SaveRefreshTokenAsync(result.Value.RefreshToken);
                }
            }
            else
            {
                await _tokenStorage.RemoveTokenAsync();
                await _secureTokenStore.ClearAsync();
            }
        }

        return result;
    }
}
