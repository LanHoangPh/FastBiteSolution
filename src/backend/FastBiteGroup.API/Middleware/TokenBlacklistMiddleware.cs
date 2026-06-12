using FastBiteGroup.Application.Abstractions.Caching;
namespace FastBiteGroup.API.Middleware;

public sealed class TokenBlacklistMiddleware(ILogger<TokenBlacklistMiddleware> logger, ICacheService cacheService)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                var isBlacklisted = await cacheService.IsTokenBlacklistedAsync(jti, context.RequestAborted);

                if (isBlacklisted)
                {
                    logger.LogWarning(
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
