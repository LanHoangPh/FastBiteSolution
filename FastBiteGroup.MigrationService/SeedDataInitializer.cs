using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence;
using FastBiteGroup.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastBiteGroup.MigrationService;

public sealed class SeedDataInitializer(
    ApplicationDbContext dbContext,
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager,
    IOptions<SeedDataOptions> options,
    ILogger<SeedDataInitializer> logger)
{
    private readonly SeedDataOptions seedOptions = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedRolesAsync();

        var adminUser = await SeedAdminUserAsync();

        if (seedOptions.SeedSampleProducts)
        {
            await SeedProductsAsync(adminUser?.Id ?? Guid.Empty, cancellationToken);
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in seedOptions.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new AppRole(roleName));
            ThrowIfFailed(result, $"create role '{roleName}'");

            logger.LogInformation("Seeded role {RoleName}.", roleName);
        }
    }

    private async Task<AppUser?> SeedAdminUserAsync()
    {
        var adminOptions = seedOptions.Admin;
        var admin = await userManager.FindByEmailAsync(adminOptions.Email);

        if (admin is null)
        {
            if (string.IsNullOrWhiteSpace(adminOptions.Password))
            {
                logger.LogWarning(
                    "Admin user was not seeded because SeedData:Admin:Password is not configured.");
                return null;
            }

            admin = new AppUser
            {
                Email = adminOptions.Email,
                UserName = adminOptions.UserName,
                FirstName = adminOptions.FirstName,
                LastName = adminOptions.LastName,
                DateOfBirth = adminOptions.DateOfBirth,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, adminOptions.Password);
            ThrowIfFailed(createResult, $"create admin user '{adminOptions.Email}'");

            logger.LogInformation("Seeded admin user {Email}.", adminOptions.Email);
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            ThrowIfFailed(roleResult, $"assign Admin role to '{admin.Email}'");

            logger.LogInformation("Assigned Admin role to {Email}.", admin.Email);
        }

        return admin;
    }

    private async Task SeedProductsAsync(Guid createdBy, CancellationToken cancellationToken)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var products = new[]
        {
            Products.Create("Classic Burger", "Beef burger with lettuce, tomato, and house sauce.", 59000m),
            Products.Create("Crispy Chicken Burger", "Crispy chicken fillet with slaw and spicy mayo.", 55000m),
            Products.Create("French Fries", "Golden fries served with ketchup.", 29000m),
            Products.Create("Iced Lemon Tea", "Fresh lemon tea served cold.", 25000m)
        };

        foreach (var product in products)
        {
            product.CreatedAt = now;
            product.CreatedBy = createdBy;
        }

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {ProductCount} sample products.", products.Length);
    }

    private static void ThrowIfFailed(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException($"Failed to {operation}. {errors}");
    }
}
