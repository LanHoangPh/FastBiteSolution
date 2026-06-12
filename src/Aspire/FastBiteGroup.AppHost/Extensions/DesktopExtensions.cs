using Aspire.Hosting.ApplicationModel;

namespace FastBiteGroup.AppHost.Extensions;

internal static class DesktopExtensions
{
    internal static void AddOptionalDesktop(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> api)
    {
        const string desktopPath = @"..\..\..\..\..\";
        var desktop = builder.Configuration[""] ?? Path.GetFullPath(desktopPath);

        if (Directory.Exists(desktop))
        {

        }

    }
}
