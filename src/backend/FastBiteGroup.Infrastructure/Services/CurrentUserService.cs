using FastBiteGroup.Application.Abstractions.Authentication;

namespace FastBiteGroup.Infrastructure.Services;

/// <summary>
/// Resolves the current user identity from the HTTP request's JWT via IHttpContextAccessor.
/// 
/// Claim mapping matches what JwtTokenService.GenerateAccessToken writes:
///   - sub / NameIdentifier  → UserId
///   - email                 → Email
///   - name                  → UserName
///   - firstName             → FirstName
///   - lastName              → LastName
///   - jti                   → Jti
///   - role                  → Roles
/// 
/// Lifetime: Scoped (one per HTTP request — matches IHttpContextAccessor behaviour).
/// </summary>
internal sealed class CurrentUserService : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _principal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
    }

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _principal.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public Guid UserId
    {
        get
        {
            var value = _principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? _principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }
    }

    /// <inheritdoc />
    public string Email =>
        _principal.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? _principal.FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    /// <inheritdoc />
    public string UserName =>
        _principal.FindFirstValue(ClaimTypes.Name)
        ?? string.Empty;

    /// <inheritdoc />
    public string FirstName =>
        _principal.FindFirstValue("firstName")
        ?? string.Empty;

    /// <inheritdoc />
    public string LastName =>
        _principal.FindFirstValue("lastName")
        ?? string.Empty;

    /// <inheritdoc />
    public string Jti =>
        _principal.FindFirstValue(JwtRegisteredClaimNames.Jti)
        ?? string.Empty;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles =>
        _principal
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public bool IsInRole(string role) =>
        _principal.IsInRole(role);
}
