namespace FastBiteGroup.Contract.Services.V1.Auth.Events;

public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string Otp,
    string MagicLinkToken);
