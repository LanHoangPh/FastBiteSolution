using Aspire.Hosting.ApplicationModel;

namespace FastBiteGroup.AppHost.Extensions;

internal static class DesktopExtensions
{
    internal static void AddOptionalDesktop(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> api)
    {
        var desktopPath = builder.Configuration["DESKTOP_PATH"]
            ?? Path.GetFullPath(Path.Combine(
                builder.AppHostDirectory,
                "..",
                "..",
                "fastbite-desktop",
                "src",
                "FastBiteGroup.Desktop"));

        if (!OperatingSystem.IsWindows() || !Directory.Exists(desktopPath))
        {
            return;
        }

        builder.AddExecutable(
                name: "desktop-wpf",
                command: "dotnet",
                workingDirectory: desktopPath,
                args: ["run"])
            .WaitFor(api)
            .WithEnvironment("FASTBITE_API_BASE_URL", api.GetEndpoint("http"));
    }
}
