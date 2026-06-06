namespace FastBiteGroup.Application.Abstractions.Emails;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default);
}
