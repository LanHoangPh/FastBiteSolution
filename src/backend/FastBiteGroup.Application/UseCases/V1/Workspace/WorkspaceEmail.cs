namespace FastBiteGroup.Application.UseCases.V1.Workspace;

internal static class WorkspaceEmail
{
    public static string Normalize(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
