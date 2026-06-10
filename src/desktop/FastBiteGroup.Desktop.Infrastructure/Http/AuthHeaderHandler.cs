using System.Net.Http.Headers;
using FastBiteGroup.Desktop.Application.Abstractions;

namespace FastBiteGroup.Desktop.Infrastructure.Http;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public AuthHeaderHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tokenProvider.HasAccessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
