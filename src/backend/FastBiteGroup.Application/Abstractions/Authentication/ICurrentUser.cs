namespace FastBiteGroup.Application.Abstractions.Authentication;

/// <summary>
/// Provides ambient identity information extracted from the current request's JWT token.
/// Implemented in Infrastructure using IHttpContextAccessor + ClaimsPrincipal.
/// 
/// Design rationale:
///   - Application layer defines this interface — zero dependency on ASP.NET HTTP primitives.
///   - Command/Query handlers inject this instead of ClaimsPrincipal directly.
///   - Scoped lifetime: one instance per HTTP request.
/// 
/// Null-safety:
///   - IsAuthenticated should be checked before accessing UserId / Email / Roles.
///   - Anonymous endpoints that use ICurrentUser should guard with IsAuthenticated.
/// </summary>
public interface ICurrentUser
{
    /// <summary>The authenticated user's ID extracted from the 'sub' claim.</summary>
    Guid UserId { get; }

    /// <summary>The authenticated user's email extracted from the 'email' claim.</summary>
    string Email { get; }

    /// <summary>The authenticated user's username extracted from the 'name' claim.</summary>
    string UserName { get; }

    /// <summary>The authenticated user's first name extracted from the 'firstName' claim.</summary>
    string FirstName { get; }

    /// <summary>The authenticated user's last name extracted from the 'lastName' claim.</summary>
    string LastName { get; }

    /// <summary>
    /// The JTI (JWT ID) claim. Useful for token blacklist checks within the same request.
    /// </summary>
    string Jti { get; }

    /// <summary>All roles assigned to the authenticated user.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>True if the request carries a valid authenticated principal.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Returns true if the current user has the specified role.</summary>
    bool IsInRole(string role);
}
