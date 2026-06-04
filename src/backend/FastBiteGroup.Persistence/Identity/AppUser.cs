using FastBiteGroup.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FastBiteGroup.Persistence.Identity;

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
