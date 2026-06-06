using FastBiteGroup.Application.Abstractions.Caching;
namespace FastBiteGroup.API.Middleware;

public sealed class TokenBlacklistMiddleware : IMiddleware
{
    private readonly ILogger<TokenBlacklistMiddleware> _logger;
    private readonly ICacheService _cacheService;

    public TokenBlacklistMiddleware(ILogger<TokenBlacklistMiddleware> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                var isBlacklisted = await _cacheService.IsTokenBlacklistedAsync(jti, context.RequestAborted);

                if (isBlacklisted)
                {
                    _logger.LogWarning(
                        "Blocked blacklisted token. Jti: {Jti}, Path: {Path}",
                        jti, context.Request.Path);

                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/problem+json";

                    var problem = new
                    {
                        Status = (int)HttpStatusCode.Unauthorized,
                        Title = "Unauthorized",
                        Detail = "Your session has been revoked. Please log in again.",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    return;
                }
            }
        }

        await next(context);
    }
}
public static class TokenBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenBlacklist(this IApplicationBuilder app)
        => app.UseMiddleware<TokenBlacklistMiddleware>();
}
