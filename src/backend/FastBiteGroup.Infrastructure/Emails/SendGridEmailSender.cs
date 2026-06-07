using FastBiteGroup.Application.Abstractions.Emails;
using FastBiteGroup.Infrastructure.DependencyInjection.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FastBiteGroup.Infrastructure.Emails;

internal sealed class SendGridEmailSender : IEmailSender
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly ISendGridClient? _client;

    public SendGridEmailSender(IOptions<SendGridOptions> options, ILogger<SendGridEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Initialize the client only if ApiKey is provided to avoid ArgumentNullException
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _client = new SendGridClient(_options.ApiKey);
        }
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        if (_client == null || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("SendGrid ApiKey is missing. Email to {To} was not sent. Subject: {Subject}", to, subject);
            return;
        }

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            HtmlContent = htmlContent
        };

        msg.AddTo(new EmailAddress(to));

        var response = await _client.SendEmailAsync(msg, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send email via SendGrid. Status: {StatusCode}, Response: {Body}", response.StatusCode, body);
        }
        else
        {
            _logger.LogInformation("Email sent successfully to {To} via SendGrid.", to);
        }
    }
}
