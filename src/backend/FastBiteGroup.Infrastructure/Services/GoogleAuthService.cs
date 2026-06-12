using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Shared;
using Google.Apis.Auth;

namespace FastBiteGroup.Infrastructure.Services;

internal sealed class GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    : IGoogleAuthService
{
    public async Task<Result<GooglePayload>> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientId = configuration["GoogleAuth:ClientId"];
            var settings = new GoogleJsonWebSignature.ValidationSettings();

            if (!string.IsNullOrEmpty(clientId) && clientId != "YOUR_GOOGLE_CLIENT_ID")
            {
                settings.Audience = [clientId];
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload is null)
            {
                return Result.Failure<GooglePayload>(new Error("GoogleAuth.InvalidToken", "Invalid Google ID Token."));
            }

            var googlePayload = new GooglePayload(
                payload.Email,
                payload.GivenName ?? "",
                payload.FamilyName ?? "",
                payload.Picture ?? ""
            );

            return googlePayload;
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Invalid Google JWT token provided.");
            return Result.Failure<GooglePayload>(new Error("GoogleAuth.InvalidJwt", "The provided Google token is invalid or expired."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Google ID Token.");
            return Result.Failure<GooglePayload>(new Error("GoogleAuth.Error", "An error occurred while validating Google token."));
        }
    }
}
