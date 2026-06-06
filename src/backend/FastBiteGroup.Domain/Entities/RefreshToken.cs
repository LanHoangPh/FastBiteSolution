using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Domain.Entities;

public class AppRefreshToken : EntityBase<long>
{
    public string Token { get; private set; } = default!;
    public string Jti { get; private set; } = default!;   // Links to the Access Token JTI
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }  // Rotation chain audit

    protected AppRefreshToken() { }

    public static AppRefreshToken Create(
        string token, string jti, Guid userId, DateTime expiresAt) =>
        new()
        {
            Token = token,
            Jti = jti,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            IsUsed = false
        };

    public void MarkUsed(string replacementToken)
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        ReplacedByToken = replacementToken;
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsActive => !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow;
}
