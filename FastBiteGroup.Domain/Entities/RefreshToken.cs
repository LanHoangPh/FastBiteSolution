using FastBiteGroup.Domain.Abstractions;

namespace FastBiteGroup.Domain.Entities;

public class RefreshToken : EntityBase<long>
{
    public string Token { get; private set; } = default!;
    public string Jti { get; private set; } = default!;   // liên kết với Access Token
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public bool IsUsed { get; private set; }
    public string? ReplacedByToken { get; private set; }  // Rotation chain

    protected RefreshToken() { }

    public static RefreshToken Create(
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
