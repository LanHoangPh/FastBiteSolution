using Aspire.Hosting.ApplicationModel;

namespace FastBiteGroup.AppHost.Extensions;

internal static class FrontendExtensions
{
    internal static void AddOptionalFrontend(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> api)
    {
        var frontendPath = builder.Configuration["FRONTEND_PATH"]
            ?? Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "fastbite-frontend"));

        if (!Directory.Exists(frontendPath))
        {
            return;
        }
    }
}
