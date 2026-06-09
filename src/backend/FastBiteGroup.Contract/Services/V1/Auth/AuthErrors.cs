using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Contract.Services.V1.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "Email or password is incorrect.");

    public static readonly Error AccountInactive = new(
        "Auth.AccountInactive",
        "Account is not active. Please confirm your email first.");

    public static readonly Error AccountLocked = new(
        "Auth.AccountLocked",
        "Account is locked. Please try again later.");

    public static readonly Error InvalidToken = new(
        "Auth.InvalidToken",
        "Access token is invalid or cannot be parsed.");

    public static readonly Error InvalidRefreshToken = new(
        "Auth.InvalidRefreshToken",
        "Refresh token not found.");

    public static readonly Error RefreshTokenExpiredOrRevoked = new(
        "Auth.RefreshTokenExpiredOrRevoked",
        "Refresh token has expired or been revoked. Please log in again.");

    public static readonly Error TokenMismatch = new(
        "Auth.TokenMismatch",
        "Access token and refresh token do not match.");

    public static readonly Error UserNotFound = new(
        "Auth.UserNotFound",
        "User not found.");

    public static readonly Error AssociatedUserNotFound = new(
        "Auth.UserNotFound",
        "Associated user was not found.");

    public static readonly Error EmailAlreadyConfirmed = new(
        "Auth.EmailAlreadyConfirmed",
        "Email is already confirmed.");

    public static readonly Error InvalidEmailConfirmationToken = new(
        "Auth.InvalidToken",
        "Invalid or expired email confirmation token.");

    public static readonly Error TooManyRequests = new(
        "Auth.TooManyRequests",
        "Too many OTP requests. Please wait before trying again.");

    public static readonly Error AccountBlocked = new(
        "Auth.AccountBlocked",
        "Too many failed OTP attempts. Please request a new OTP.");

    public static readonly Error InvalidOtp = new(
        "Auth.InvalidOtp",
        "OTP is invalid or has expired.");

    public static Error EmailAlreadyExists(string email) =>
        new("Auth.EmailAlreadyExists", $"Email '{email}' is already registered.");

    public static Error RegistrationFailed(string message) =>
        new("Auth.RegistrationFailed", message);

    public static Error ResetFailed(string message) =>
        new("Auth.ResetFailed", message);

    public static Error GoogleRegistrationFailed(string? message) =>
        new("GoogleLogin.RegistrationFailed", message ?? "Auto-registration failed.");
}
