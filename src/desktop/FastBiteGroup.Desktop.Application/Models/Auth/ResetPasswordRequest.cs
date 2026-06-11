namespace FastBiteGroup.Desktop.Application.Models.Auth;

public sealed record ResetPasswordRequest(string Email, string Otp, string NewPassword);
