using FastBiteGroup.Application.Abstractions.Emails;
using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Contract.Services.V1.Auth.Events;
using System.Net;
using System.Text.Json;

namespace FastBiteGroup.Infrastructure.BackgroundJobs;

public sealed class IntegrationOutboxProcessorService(
    IServiceProvider serviceProvider,
    ILogger<IntegrationOutboxProcessorService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("IntegrationOutboxProcessorService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred processing outbox messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var outboxStore = scope.ServiceProvider.GetRequiredService<IIntegrationOutboxStore>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var messages = await outboxStore.GetPendingAsync(batchSize: 20, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type == nameof(UserRegisteredIntegrationEvent))
                {
                    var payload = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(message.Payload);
                    if (payload != null)
                    {
                        var email = WebUtility.UrlEncode(payload.Email);
                        var token = WebUtility.UrlEncode(payload.EmailConfirmationToken);
                        var verificationUrl = $"https://localhost:5001/api/v1/auth/verify-email?email={email}&token={token}";

                        var htmlBody = $@"
                            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
                                <h2 style=""color: #222;"">Confirm your FastBite account</h2>
                                <p>Hello,</p>
                                <p>You recently created a FastBite account. Click the button below to confirm your email address.</p>
                                <p style=""margin: 24px 0;"">
                                    <a href=""{verificationUrl}"" style=""background-color: #111827; color: #ffffff; padding: 12px 18px; text-decoration: none; border-radius: 6px; display: inline-block;"">Confirm email</a>
                                </p>
                                <p style=""color: #555; font-size: 14px;"">If you did not create a FastBite account, you can safely ignore this email.</p>
                                <hr style=""border: 0; border-top: 1px solid #eee; margin: 20px 0;"" />
                                <p style=""color: #999; font-size: 12px;"">&copy; {DateTime.UtcNow.Year} FastBite. All rights reserved.</p>
                            </div>
                        ";

                        await emailSender.SendEmailAsync(
                            payload.Email,
                            "FastBite - Activate your account",
                            htmlBody,
                            cancellationToken);
                    }
                }
                else if (message.Type == nameof(PasswordResetOtpRequestedIntegrationEvent))
                {
                    var payload = JsonSerializer.Deserialize<PasswordResetOtpRequestedIntegrationEvent>(message.Payload);
                    if (payload != null)
                    {
                        var htmlBody = $@"
                            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;"">
                                <h2 style=""color: #333;"">Password Reset Request</h2>
                                <p>You recently requested to reset your password for your FastBite account. Use the OTP below to proceed:</p>
                                <div style=""background-color: #f4f4f4; padding: 15px; text-align: center; border-radius: 4px; margin: 20px 0;"">
                                    <strong style=""font-size: 24px; letter-spacing: 2px; color: #007bff;"">{payload.Otp}</strong>
                                </div>
                                <p style=""color: #555; font-size: 14px;"">If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                                <hr style=""border: 0; border-top: 1px solid #eee; margin: 20px 0;"" />
                                <p style=""color: #999; font-size: 12px; text-align: center;"">&copy; {DateTime.UtcNow.Year} FastBite. All rights reserved.</p>
                            </div>
                        ";

                        await emailSender.SendEmailAsync(
                            payload.Email,
                            "FastBite - Reset your password",
                            htmlBody,
                            cancellationToken);
                    }
                }

                await outboxStore.MarkProcessedAsync(message.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                await outboxStore.MarkFailedAsync(message.Id, ex.ToString(), cancellationToken);
            }
        }
    }
}
