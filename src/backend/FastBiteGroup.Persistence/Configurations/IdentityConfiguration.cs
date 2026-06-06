using Microsoft.AspNetCore.Identity;

namespace FastBiteGroupMCA.Persistentce.Configurations;

internal sealed class IdentityRolsConfiguration : IEntityTypeConfiguration<AppRoles>
{
    public void Configure(EntityTypeBuilder<AppRoles> builder)
    {
        builder.ToTable(TableNames.AppRoles);
    }
}
internal sealed class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable(TableNames.AppRoleClaims);
        builder.HasKey(rc => rc.Id);
    }
}
internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable(TableNames.AppUserRoles);

        // Khóa chính composite
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}
internal sealed class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable(TableNames.AppUserClaims);
    }
}
internal sealed class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable(TableNames.AppUserLogins);

        // Khóa chính composite
        builder.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
    }
}
internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable(TableNames.AppUserTokens);

        // Khóa chính composite
        builder.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
    }
}

