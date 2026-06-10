using System.Security.Cryptography;
using System.Text;
using FastBiteGroup.Desktop.Application.Abstractions;

namespace FastBiteGroup.Desktop.Infrastructure.Storage;

public class DpapiSecureTokenStore : ISecureTokenStore
{
    private readonly string _filePath;

    public DpapiSecureTokenStore()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FastBite");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "refresh_token.dat");
    }

    public async Task SaveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(refreshToken), null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_filePath, encryptedBytes, cancellationToken);
    }

    public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath)) return null;

        try
        {
            var encryptedBytes = await File.ReadAllBytesAsync(_filePath, cancellationToken);
            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
        return Task.CompletedTask;
    }
}
