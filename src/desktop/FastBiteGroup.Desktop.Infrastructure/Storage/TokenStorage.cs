using FastBiteGroup.Desktop.Application.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace FastBiteGroup.Desktop.Infrastructure.Storage;

public class TokenStorage : ITokenStorage
{
    private readonly string _filePath;

    public TokenStorage()
    {
        var folder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "FastBite");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "auth.dat");
    }

    public async Task SaveTokenAsync(string token)
    {
        var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_filePath, encryptedBytes);
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!File.Exists(_filePath)) return null;

        try
        {
            var encryptedBytes = await File.ReadAllBytesAsync(_filePath);
            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return null;
        }
    }

    public Task RemoveTokenAsync()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
        return Task.CompletedTask;
    }
}
