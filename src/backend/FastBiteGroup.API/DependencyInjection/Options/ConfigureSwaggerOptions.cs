namespace FastBiteGroup.API.DependencyInjection.Options;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = AppDomain.CurrentDomain.FriendlyName,
                Version = description.ApiVersion.ToString()
            });
        }

        options.MapType<DateOnly>(() => new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Format = "date"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Paste JWT access token only. Do not include the 'Bearer ' prefix.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document, null),
                []
            }
        });

        options.CustomSchemaIds(type => type.ToString().Replace("+", "."));
    }
}


