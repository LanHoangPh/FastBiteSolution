namespace FastBiteGroup.Application.Abstractions.Authentication;

public record GooglePayload(string Email, string FirstName, string LastName, string Picture);

public interface IGoogleAuthService
{
    Task<Result<GooglePayload>> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
