namespace FastBiteGroup.Application.Abstractions.Authentication;

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
