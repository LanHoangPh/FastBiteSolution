using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Persistence.Identity;

namespace FastBiteGroup.Infrastructure.Services;

internal sealed class UserAuthService(UserManager<AppUser> userManager) : IUserAuthService
{
    public async Task<UserDto?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : MapToDto(user, await userManager.GetRolesAsync(user));
    }

    public async Task<UserDto?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user is null ? null : MapToDto(user, await userManager.GetRolesAsync(user));
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var isMatch = await userManager.CheckPasswordAsync(user, password);
        if (!isMatch)
        {
            await userManager.AccessFailedAsync(user);
        }
        else
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }

        return isMatch;
    }

    public async Task<bool> IsLockedOutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;
        return await userManager.IsLockedOutAsync(user);
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

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return (null, error);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return (null, FormatIdentityErrors(roleResult));
        }

        // Reload roles for the DTO (AddToRoleAsync does not populate the user's roles in memory)
        var roles = await userManager.GetRolesAsync(user);
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
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Generate a random secure password for Google users
        var randomPassword = Guid.NewGuid().ToString("N") + "Aa1!";

        var result = await userManager.CreateAsync(user, randomPassword);
        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return (null, error);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return (null, FormatIdentityErrors(roleResult));
        }

        var roles = await userManager.GetRolesAsync(user);

        return (MapToDto(user, roles), null);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return string.Empty;
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailWithTokenAsync(string email, string token, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return false;
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            user.IsActive = true;
            await userManager.UpdateAsync(user);
        }
        return result.Succeeded;
    }

    public async Task<bool> ActivateUserAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return false;

        user.EmailConfirmed = true;
        user.IsActive = true;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.Failure(new Error("UserAuth.UserNotFound", "User not found"));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        return Result.Failure(new Error("UserAuth.ResetFailed", FormatIdentityErrors(result)));
    }

    private static string FormatIdentityErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(e => e.Description));

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
