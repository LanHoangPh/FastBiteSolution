using Microsoft.EntityFrameworkCore;

namespace FastBiteGroup.Persistence;

public sealed class ApplicationDbContext : DbContext
{


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}
