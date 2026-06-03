using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Domain.Entities;

/// <summary>
/// Refresh token for JWT rotation. Stored in DB and linked to AppUser via UserId.
/// Navigation property to AppUser is intentionally omitted to keep Domain pure.
/// The FK relationship is configured in Persistence via RefreshTokenConfiguration.
/// </summary>
public class AppRefreshToken : EntityBase<long>
{
    public string Token { get; private set; } = default!;
    public string Jti { get; private set; } = default!;   // Links to the Access Token JTI
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public bool IsUsed { get; private set; }
    public string? ReplacedByToken { get; private set; }  // Rotation chain audit

    protected AppRefreshToken() { }

    public static AppRefreshToken Create(
        string token, string jti, Guid userId, DateTime expiresAt) =>
        new()
        {
            Token = token,
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            IsUsed = false
        };

    public void MarkUsed(string replacementToken)
    {
        IsUsed = true;
        ReplacedByToken = replacementToken;
    }

    public void Revoke() => IsRevoked = true;

    public bool IsActive => !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow;
}
