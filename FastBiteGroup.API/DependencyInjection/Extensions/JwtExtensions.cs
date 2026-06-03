using FastBiteGroup.Infrastructure.DependencyInjection.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FastBiteGroup.API.DependencyInjection.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })

        .AddJwtBearer(options =>
        {
            JwtOptions jwtOption = new JwtOptions();
            configuration.GetSection(nameof(JwtOptions)).Bind(jwtOption);

            options.SaveToken = true;
            var Key = Encoding.UTF8.GetBytes(jwtOption.SecretKey);

            //options.RequireHttpsMetadata = false; // set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOption.Issuer,
                ValidAudience = jwtOption.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                        context.Response.Headers.Append("WWW-Authenticate", "Bearer error=\"invalid_token\", error_description=\"The access token expired\"");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Detail = context.ErrorDescription ?? "You are not authorized to access this resource.",
                        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    };

                    await context.Response.WriteAsJsonAsync(problemDetails);
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
