using FastBiteGroup.Desktop.Application.Abstractions;
using System.Net.Http.Headers;

namespace FastBiteGroup.Desktop.Infrastructure.ApiClients;

public class JwtAuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;

    public JwtAuthHeaderHandler(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
