namespace FastBiteGroup.MigrationService;

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public string[] Roles { get; set; } = ["Admin", "Customer"];

    public AdminUserSeedOptions Admin { get; set; } = new();

    public bool SeedSampleProducts { get; set; } = true;
}

public sealed class AdminUserSeedOptions
{
    public string Email { get; set; } = "admin@fastbite.local";
    public string UserName { get; set; } = "admin@fastbite.local";
    public string FirstName { get; set; } = "FastBite";
    public string LastName { get; set; } = "Admin";
    public DateTime DateOfBirth { get; set; } = new(1990, 1, 1);
    public string Password { get; set; } = string.Empty;
}
