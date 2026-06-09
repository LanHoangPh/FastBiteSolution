namespace FastBiteGroup.Desktop.Application.Abstractions;

public interface ITokenStorage
{
    Task SaveTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task RemoveTokenAsync();
}
