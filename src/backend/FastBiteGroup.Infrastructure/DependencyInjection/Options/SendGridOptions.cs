namespace FastBiteGroup.Infrastructure.DependencyInjection.Options;

public sealed class SendGridOptions
{
    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
