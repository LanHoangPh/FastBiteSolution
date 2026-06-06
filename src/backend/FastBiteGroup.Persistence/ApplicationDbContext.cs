using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FastBiteGroup.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<AppUser, AppRoles, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    public DbSet<AppRefreshToken> RefreshTokens => Set<AppRefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
