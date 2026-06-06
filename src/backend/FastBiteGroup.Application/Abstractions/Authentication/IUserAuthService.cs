using FastBiteGroup.Contract.Abstractions.Shared;

namespace FastBiteGroup.Application.Abstractions.Authentication;

/// <summary>
/// User data projection — avoids coupling Application to Persistence/AppUser.
/// Contains all user fields needed by Application layer use cases.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string? FullName,
    string? AvatarUrl,
    string? Bio,
    bool EmailConfirmed,
    bool IsActive,
    DateTime? LastSeenAt,
    IList<string> Roles);

/// <summary>
/// User authentication service — decouples Application from ASP.NET Identity and Persistence.
/// Implemented in Infrastructure (using UserManager internally).
/// </summary>
public interface IUserAuthService
{
    Task<UserDto?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<UserDto?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct = default);
    Task<bool> IsLockedOutAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new user and assigns the Customer role.
    /// Returns the created UserDto directly to avoid a second DB round-trip.
    /// </summary>
    Task<(UserDto? User, string? ErrorMessage)> CreateUserAsync(
        string email, string password, string firstName, string lastName,
        DateTime dateOfBirth, CancellationToken ct = default);

    /// <summary>
    /// Creates a new user from Google payload with a random secure password and confirmed email.
    /// </summary>
    Task<(UserDto? User, string? ErrorMessage)> CreateUserFromGoogleAsync(
        string email, string firstName, string lastName, string picture, CancellationToken ct = default);

    Task<string> GenerateEmailConfirmationTokenAsync(string email, CancellationToken ct = default);
    Task<bool> ConfirmEmailWithTokenAsync(string email, string token, CancellationToken ct = default);
    Task<bool> ActivateUserAsync(string email, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default);
}
