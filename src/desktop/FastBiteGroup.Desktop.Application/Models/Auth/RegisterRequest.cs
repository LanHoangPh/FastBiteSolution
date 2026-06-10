namespace FastBiteGroup.Desktop.Application.Models.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime DayOfBirth);
