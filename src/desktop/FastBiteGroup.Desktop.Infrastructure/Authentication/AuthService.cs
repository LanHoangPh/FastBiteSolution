using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.Domain.Models.Shared;
using FastBiteGroup.Desktop.Infrastructure.ApiClients;
using Refit;

namespace FastBiteGroup.Desktop.Infrastructure.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly IAuthClient _authClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly ISecureTokenStore _secureTokenStore;
    private readonly ITokenProvider _tokenProvider;
    private readonly ICurrentUserService _currentUserInfoService;

    public AuthService(
        IAuthClient authClient,
        ITokenStorage tokenStorage,
        ISecureTokenStore secureTokenStore,
        ITokenProvider tokenProvider,
        ICurrentUserService currentUserInfoService)
    {
        _authClient = authClient;
        _tokenStorage = tokenStorage;
        _secureTokenStore = secureTokenStore;
        _tokenProvider = tokenProvider;
        _currentUserInfoService = currentUserInfoService;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _authClient.RegisterAsync(request, cancellationToken);
            return Result.Success(response);
        }
        catch (ApiException ex)
        {
            var errorMessage = ApiErrorParser.GetErrorMessage(ex.Content);
            return Result.Failure<RegisterResponse>(new Error(ex.StatusCode.ToString(), errorMessage));
        }
        catch (Exception ex)
        {
            return Result.Failure<RegisterResponse>(new Error("ConnectionError", "Could not connect to server. Please try again."));
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _authClient.LoginAsync(request, cancellationToken);

            // Save tokens securely using DPAPI
            await _tokenStorage.SaveTokenAsync(response.AccessToken);
            await _secureTokenStore.SaveRefreshTokenAsync(response.RefreshToken, cancellationToken);

            // Update in-memory runtime caches
            _tokenProvider.SetAccessToken(response.AccessToken, response.AccessTokenExpiresAt);
            if (_currentUserInfoService is CurrentUserService service)
            {
                service.SetUser(response.User);
            }

            return Result.Success(response);
        }
        catch (ApiException ex)
        {
            var errorMessage = ApiErrorParser.GetErrorMessage(ex.Content);
            return Result.Failure<AuthResponse>(new Error(ex.StatusCode.ToString(), errorMessage));
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthResponse>(new Error("ConnectionError", "Could not connect to server. Please try again."));
        }
    }

    public async Task<bool> TryAutoLoginAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenStorage.GetTokenAsync();
        var refreshToken = await _secureTokenStore.GetRefreshTokenAsync(cancellationToken);

        if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        // Set the token provider in-memory reference so the LoadCurrentUserAsync call is authenticated
        if (!string.IsNullOrEmpty(accessToken))
        {
            _tokenProvider.SetAccessToken(accessToken, DateTimeOffset.MaxValue);
            await _currentUserInfoService.LoadCurrentUserAsync(cancellationToken);

            if (_currentUserInfoService.IsAuthenticated)
            {
                return true;
            }
        }

        // Access token is invalid/expired, try token rotation via refresh token
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                var response = await _authClient.RefreshAsync(
                    new RefreshTokenRequest(accessToken ?? string.Empty, refreshToken),
                    cancellationToken);

                // Save rotated tokens securely
                await _tokenStorage.SaveTokenAsync(response.AccessToken);
                await _secureTokenStore.SaveRefreshTokenAsync(response.RefreshToken, cancellationToken);

                // Update in-memory caches
                _tokenProvider.SetAccessToken(response.AccessToken, response.AccessTokenExpiresAt);
                if (_currentUserInfoService is CurrentUserService service)
                {
                    service.SetUser(response.User);
                }

                return true;
            }
            catch
            {
                // Refresh failed or revoked, perform cleanup
                await LogoutAsync(cancellationToken);
                return false;
            }
        }

        return false;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var refreshToken = await _secureTokenStore.GetRefreshTokenAsync(cancellationToken);

        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                await _authClient.LogoutAsync(new LogoutRequest(refreshToken), cancellationToken);
            }
            catch
            {
                // Ignored - still perform local cleanup even if logout request fails
            }
        }

        // Clear all local token stores and in-memory caches
        await _tokenStorage.RemoveTokenAsync();
        await _secureTokenStore.ClearAsync(cancellationToken);
        _tokenProvider.Clear();
        _currentUserInfoService.Clear();
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            await _authClient.ForgotPasswordAsync(new ForgotPasswordRequest(email), cancellationToken);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            var errorMessage = ApiErrorParser.GetErrorMessage(ex.Content);
            return Result.Failure(new Error(ex.StatusCode.ToString(), errorMessage));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ConnectionError", "Could not connect to server. Please try again."));
        }
    }

    public async Task<Result> ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            await _authClient.ResetPasswordAsync(new ResetPasswordRequest(email, otp, newPassword), cancellationToken);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            var errorMessage = ApiErrorParser.GetErrorMessage(ex.Content);
            return Result.Failure(new Error(ex.StatusCode.ToString(), errorMessage));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ConnectionError", "Could not connect to server. Please try again."));
        }
    }
}
