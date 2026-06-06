using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace FastBiteGroup.Infrastructure.Services;

/// <summary>
/// Implements IUserAuthService using ASP.NET Core Identity's UserManager.
/// Lives in Infrastructure to keep Application free of Identity/Persistence coupling.
/// </summary>
internal sealed class UserAuthService : IUserAuthService
{
    private readonly UserManager<AppUser> _userManager;

    public UserAuthService(UserManager<AppUser> userManager)
        => _userManager = userManager;

    public async Task<UserDto?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : MapToDto(user, await _userManager.GetRolesAsync(user));
    }

    public async Task<UserDto?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        return user is null ? null : MapToDto(user, await _userManager.GetRolesAsync(user));
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> IsLockedOutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;
        return await _userManager.IsLockedOutAsync(user);
    }

    /// <inheritdoc />
    public async Task<(UserDto? User, string? ErrorMessage)> CreateUserAsync(
        string email, string password, string firstName, string lastName,
        DateTime dateOfBirth, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}".Trim(),
            DateOfBirth = dateOfBirth,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return (null, error);
        }

        // Assign default Customer role
        await _userManager.AddToRoleAsync(user, "Customer");

        // Reload roles for the DTO (AddToRoleAsync does not populate the user's roles in memory)
        var roles = await _userManager.GetRolesAsync(user);
        return (MapToDto(user, roles), null);
    }

    private static UserDto MapToDto(AppUser user, IList<string> roles) =>
        new(
            Id: user.Id,
            Email: user.Email!,
            UserName: user.UserName!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: user.FullName,
            AvatarUrl: user.AvatarUrl,
            Bio: user.Bio,
            IsActive: user.IsActive,
            LastSeenAt: user.LastSeenAt,
            Roles: roles);
}
