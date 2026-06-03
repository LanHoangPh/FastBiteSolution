namespace FastBiteGroup.Application.Abstractions.Caching;

public interface ICacheService
{
    /// <summary>Serialize and store a value under the given key with an absolute expiry.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>Retrieve and deserialize a value. Returns null if key does not exist.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Remove a key from the cache.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Blacklist a JWT by its jti claim.
    /// TTL is set to the remaining lifetime so the key auto-expires when the token would have expired anyway.
    /// </summary>
    Task BlacklistTokenAsync(string jti, TimeSpan remainingLifetime, CancellationToken ct = default);

    /// <summary>Returns true if the given jti has been blacklisted (token was logged out).</summary>
    Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken ct = default);
}
