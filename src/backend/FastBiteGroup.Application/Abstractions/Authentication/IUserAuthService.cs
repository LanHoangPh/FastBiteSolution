namespace FastBiteGroup.Application.Abstractions.Authentication;

/// <summary>
/// User data projection — avoids coupling Application to Persistence/AppUser.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
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
    Task<(bool Success, string? ErrorMessage)> CreateUserAsync(
        string email, string password, string firstName, string lastName,
        DateTime dateOfBirth, CancellationToken ct = default);
}
