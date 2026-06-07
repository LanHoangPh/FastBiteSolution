using FastBiteGroup.Application.Abstractions.Authentication;

namespace FastBiteGroup.Infrastructure.Services;

internal sealed class OtpService : IOtpService
{
    private readonly IDatabase _db;

    public OtpService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task<string> GenerateOtpAsync(string purpose, string identifier, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var key = GetOtpKey(purpose, identifier);
        var attemptsKey = GetAttemptsKey(purpose, identifier);
        var timeToLive = expiry ?? TimeSpan.FromMinutes(5);

        // Set OTP and clear any existing attempt counters
        await _db.StringSetAsync(key, otp, timeToLive);
        await _db.KeyDeleteAsync(attemptsKey);

        return otp;
    }

    public async Task<OtpValidationResult> ValidateOtpAsync(string purpose, string identifier, string code, int maxAttempts = 5, CancellationToken ct = default)
    {
        var key = GetOtpKey(purpose, identifier);
        var attemptsKey = GetAttemptsKey(purpose, identifier);

        var existingOtp = await _db.StringGetAsync(key);

        if (existingOtp.IsNullOrEmpty)
        {
            return OtpValidationResult.ExpiredOrNotFound;
        }

        // Increment attempts atomically
        var attempts = await _db.StringIncrementAsync(attemptsKey);

        // Ensure attempts key expires roughly when the OTP expires
        if (attempts == 1)
        {
            var ttl = await _db.KeyTimeToLiveAsync(key);
            if (ttl.HasValue)
            {
                await _db.KeyExpireAsync(attemptsKey, ttl.Value);
            }
        }

        if (attempts > maxAttempts)
        {
            await InvalidateOtpAsync(purpose, identifier, ct);
            return OtpValidationResult.MaxAttemptsReached;
        }

        if (existingOtp.ToString() == code)
        {
            await InvalidateOtpAsync(purpose, identifier, ct);
            return OtpValidationResult.Success;
        }

        return OtpValidationResult.InvalidCode;
    }

    public async Task InvalidateOtpAsync(string purpose, string identifier, CancellationToken ct = default)
    {
        var key = GetOtpKey(purpose, identifier);
        var attemptsKey = GetAttemptsKey(purpose, identifier);

        await _db.KeyDeleteAsync(new RedisKey[] { key, attemptsKey });
    }

    private static string GetOtpKey(string purpose, string identifier) =>
        $"otp:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}";

    private static string GetAttemptsKey(string purpose, string identifier) =>
        $"otp_attempts:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}";
}
