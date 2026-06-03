using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FastBiteGroup.API.DependencyInjection.Options;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = AppDomain.CurrentDomain.FriendlyName,
                Version = description.ApiVersion.ToString()
            });
        }

        options.MapType<DateOnly>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "date",
            Example = new OpenApiString(DateTime.Now.ToString("yyyy-MM-dd"))
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Nhập token theo định dạng: Bearer {your_token}",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        options.CustomSchemaIds(type => type.ToString().Replace("+", "."));
    }
}
