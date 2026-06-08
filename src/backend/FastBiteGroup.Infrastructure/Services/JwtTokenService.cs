using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Infrastructure.DependencyInjection.Options;

namespace FastBiteGroup.Infrastructure.Services;
internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
        => _options = options.Value;

    /// <inheritdoc />
    public (string Token, string Jti, DateTime ExpiresAt) GenerateAccessToken(
        Guid userId,
        string email,
        string userName,
        string firstName,
        string lastName,
        IEnumerable<string> roles)
    {
        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("firstName", firstName),
            new("lastName", lastName),
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return (token, jti, expiresAt);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc />
    public string? GetJtiFromExpiredToken(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,   // intentionally skip lifetime validation
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = key
            };

            handler.ValidateToken(accessToken, validationParams, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwt)
                return jwt.Id;

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public TimeSpan GetAccessTokenRemainingLifetime(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,   // parse even if expired — we compute remaining ourselves
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = key
            };

            handler.ValidateToken(accessToken, validationParams, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwt)
            {
                var remaining = jwt.ValidTo - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            return TimeSpan.Zero;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}
