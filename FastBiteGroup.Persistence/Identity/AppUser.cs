using FastBiteGroup.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FastBiteGroup.Persistence.Identity;

/// <summary>
/// Application user entity — extends ASP.NET Core Identity's IdentityUser with domain profile fields.
/// Lives in Persistence because it depends on Microsoft.AspNetCore.Identity.EntityFrameworkCore.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation property
    public virtual ICollection<AppRefreshToken> RefreshTokens { get; set; } = [];
}

public class AppRole : IdentityRole<Guid>
{
    public AppRole() { }
    public AppRole(string roleName) : base(roleName) { }
}
