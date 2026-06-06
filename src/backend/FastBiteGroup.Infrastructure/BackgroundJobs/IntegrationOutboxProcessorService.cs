using System.Text.Json;
using FastBiteGroup.Application.Abstractions.Emails;
using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Contract.Services.V1.Auth.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FastBiteGroup.Infrastructure.BackgroundJobs;

public sealed class IntegrationOutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IntegrationOutboxProcessorService> _logger;

    public IntegrationOutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<IntegrationOutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IntegrationOutboxProcessorService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred processing outbox messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
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
                        var htmlBody = $@"
                            <h2>Welcome to FastBite!</h2>
                            <p>Here is your 6-digit OTP: <strong>{payload.Otp}</strong></p>
                            <p>Or click this link to verify your email automatically:</p>
                            <a href=""https://localhost:5001/api/v1/auth/verify-email?email={payload.Email}&code={payload.MagicLinkToken}"">Verify Account</a>
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
                _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
                await outboxStore.MarkFailedAsync(message.Id, ex.ToString(), cancellationToken);
            }
        }
    }
}
