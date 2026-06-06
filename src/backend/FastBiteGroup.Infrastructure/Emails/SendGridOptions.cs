namespace FastBiteGroup.Infrastructure.Emails;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";
    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
