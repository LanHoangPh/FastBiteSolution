using Aspire.Hosting.ApplicationModel;

namespace FastBiteGroup.AppHost.Extensions;

internal static class DesktopExtensions
{
    internal static void AddOptionalDesktop(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> api)
    {
        var desktopPath = builder.Configuration["DESKTOP_PATH"]
            ?? Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "desktop_tauri", "fasbitegroup.desktop"));

        if (Directory.Exists(desktopPath))
        {
            var runDesktop = builder.Configuration["RUN_DESKTOP"] == "true";

            if (runDesktop)
            {
                builder.AddExecutable("desktop-app", "pnpm", desktopPath, "tauri", "dev")
                    .WithReference(api)
                    .ExcludeFromManifest();
            }
            else
            {
                var webApp = builder.AddExecutable("web-app", "pnpm", desktopPath, "dev")
                    .WithReference(api)
                    .WithHttpEndpoint(port: 1420, name: "frontend", isProxied: false)
                    .ExcludeFromManifest();

                api.WithEnvironment("ApiSettings__FrontendUrl", webApp.GetEndpoint("frontend"));
            }
        }
    }
}
