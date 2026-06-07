using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Persistence.Identity;

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

        var isMatch = await _userManager.CheckPasswordAsync(user, password);
        if (!isMatch)
        {
            await _userManager.AccessFailedAsync(user);
        }
        else
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        return isMatch;
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
            EmailConfirmed = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return (null, error);
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        // Reload roles for the DTO (AddToRoleAsync does not populate the user's roles in memory)
        var roles = await _userManager.GetRolesAsync(user);
        return (MapToDto(user, roles), null);
    }

    public async Task<(UserDto? User, string? ErrorMessage)> CreateUserFromGoogleAsync(
        string email, string firstName, string lastName, string picture, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}".Trim(),
            AvatarUrl = picture,
            EmailConfirmed = true, // Auto confirmed from Google
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Generate a random secure password for Google users
        var randomPassword = Guid.NewGuid().ToString("N") + "Aa1!";

        var result = await _userManager.CreateAsync(user, randomPassword);
        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return (null, error);
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        var roles = await _userManager.GetRolesAsync(user);

        return (MapToDto(user, roles), null);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return string.Empty;
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailWithTokenAsync(string email, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
        }
        return result.Succeeded;
    }

    public async Task<bool> ActivateUserAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;

        user.EmailConfirmed = true;
        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.Failure(new Error("UserAuth.UserNotFound", "User not found"));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        var error = string.Join(", ", result.Errors.Select(e => e.Description));
        return Result.Failure(new Error("UserAuth.ResetFailed", error));
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
            EmailConfirmed: user.EmailConfirmed,
            IsActive: user.IsActive,
            LastSeenAt: user.LastSeenAt,
            Roles: roles);
}
