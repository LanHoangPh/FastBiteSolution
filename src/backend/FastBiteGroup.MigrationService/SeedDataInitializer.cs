using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence;
using FastBiteGroup.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FastBiteGroup.MigrationService;

public sealed class SeedDataInitializer(
    ApplicationDbContext dbContext,
    RoleManager<AppRoles> roleManager,
    UserManager<AppUser> userManager,
    IOptions<SeedDataOptions> options,
    ILogger<SeedDataInitializer> logger)
{
    private readonly SeedDataOptions seedOptions = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedRolesAsync();

        var adminUser = await SeedAdminUserAsync();

        await SeedGlobalSettingsAsync(cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in seedOptions.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new AppRoles(roleName));
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
                FullName = $"{adminOptions.FirstName} {adminOptions.LastName}".Trim(),
                DateOfBirth = adminOptions.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
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
    private async Task SeedGlobalSettingsAsync(CancellationToken cancellationToken)
    {
        bool settingsChanged = false;

        if (!dbContext.GlobalSettings.Any(s => s.SettingKey == "DefaultCurrency"))
        {
            dbContext.GlobalSettings.Add(new FastBiteGroup.Domain.Entities.GlobalSettings
            {
                SettingKey = "DefaultCurrency",
                SettingValue = "VND"
            });
            settingsChanged = true;
            logger.LogInformation("Seeded Global Setting: DefaultCurrency = VND");
        }

        if (!dbContext.GlobalSettings.Any(s => s.SettingKey == "MaxUploadSize"))
        {
            dbContext.GlobalSettings.Add(new GlobalSettings
            {
                SettingKey = "MaxUploadSize",
                SettingValue = "5MB"
            });
            settingsChanged = true;
            logger.LogInformation("Seeded Global Setting: MaxUploadSize = 5MB");
        }

        if (settingsChanged)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
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
