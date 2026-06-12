using FastBiteGroup.Application.Abstractions.Caching;
using System.Text.Json;

namespace FastBiteGroup.Infrastructure.Services;

internal sealed class RedisCacheService(IConnectionMultiplexer connectionMultiplexer) : ICacheService
{
    private readonly IDatabase _db = connectionMultiplexer.GetDatabase();

    private const string BlacklistKeyPrefix = "auth:blacklist:jti:";

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry);
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(key);

    /// <inheritdoc />
    public async Task BlacklistTokenAsync(string jti, TimeSpan remainingLifetime, CancellationToken ct = default)
    {
        // Store "1" as a sentinel value — we only need key existence for blacklist check
        var key = $"{BlacklistKeyPrefix}{jti}";
        await _db.StringSetAsync(key, "1", remainingLifetime);
    }

    /// <inheritdoc />
    public async Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        var key = $"{BlacklistKeyPrefix}{jti}";
        return await _db.KeyExistsAsync(key);
    }
}
