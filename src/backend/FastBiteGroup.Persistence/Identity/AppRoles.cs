using Microsoft.AspNetCore.Identity;

namespace FastBiteGroup.Persistence.Identity;

public class AppRoles : IdentityRole<Guid>
{
    public bool IsSystemRole { get; set; }
    public AppRoles() { }
    public AppRoles(string roleName) : base(roleName) { }
}
