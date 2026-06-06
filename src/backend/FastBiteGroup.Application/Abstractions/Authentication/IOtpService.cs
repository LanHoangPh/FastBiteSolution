namespace FastBiteGroup.Application.Abstractions.Authentication;

public enum OtpValidationResult
{
    Success,
    InvalidCode,
    ExpiredOrNotFound,
    MaxAttemptsReached
}

public interface IOtpService
{
    /// <summary>
    /// Generates a random OTP and stores it securely.
    /// </summary>
    /// <param name="purpose">The context of the OTP (e.g. "REGISTER", "RESET_PWD").</param>
    /// <param name="identifier">The user identifier (e.g. email or phone).</param>
    /// <param name="expiry">Optional expiration time. Defaults to 5 minutes if not provided.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The generated OTP code.</returns>
    Task<string> GenerateOtpAsync(string purpose, string identifier, TimeSpan? expiry = null, CancellationToken ct = default);

    /// <summary>
    /// Validates an OTP and tracks failed attempts securely.
    /// </summary>
    /// <param name="purpose">The context of the OTP (e.g. "REGISTER", "RESET_PWD").</param>
    /// <param name="identifier">The user identifier (e.g. email or phone).</param>
    /// <param name="code">The OTP code provided by the user.</param>
    /// <param name="maxAttempts">Maximum allowed failed attempts before the OTP is blocked.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the validation.</returns>
    Task<OtpValidationResult> ValidateOtpAsync(string purpose, string identifier, string code, int maxAttempts = 5, CancellationToken ct = default);

    /// <summary>
    /// Manually invalidates (deletes) an OTP and its attempt counter.
    /// </summary>
    Task InvalidateOtpAsync(string purpose, string identifier, CancellationToken ct = default);
}
