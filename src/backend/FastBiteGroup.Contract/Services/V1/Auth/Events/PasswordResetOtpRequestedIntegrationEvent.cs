namespace FastBiteGroup.Contract.Services.V1.Auth.Events;

public sealed record PasswordResetOtpRequestedIntegrationEvent(
    Guid Id,
    string Email,
    string Otp);
