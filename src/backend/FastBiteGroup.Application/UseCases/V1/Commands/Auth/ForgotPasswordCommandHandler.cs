using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Events;
using System.Text.Json;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUserAuthService _userAuthService;
    private readonly ICacheService _cacheService;
    private readonly IOtpService _otpService;
    private readonly IIntegrationOutboxStore _outboxStore;

    public ForgotPasswordCommandHandler(
        IUserAuthService userAuthService,
        ICacheService cacheService,
        IOtpService otpService,
        IIntegrationOutboxStore outboxStore)
    {
        _userAuthService = userAuthService;
        _cacheService = cacheService;
        _otpService = otpService;
        _outboxStore = outboxStore;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Success();
        }

        // Spam prevention: limit requests to max 3 per 15 minutes
        string countKey = $"OTP_RESET_COUNT_{user.Email}";
        var count = await _cacheService.GetAsync<int>(countKey, cancellationToken);
        if (count >= 3)
        {
            return Result.Failure(AuthErrors.TooManyRequests);
        }

        await _cacheService.SetAsync(countKey, count + 1, TimeSpan.FromMinutes(15), cancellationToken);

        // Generate 6-digit OTP using IOtpService (10 mins TTL)
        var otp = await _otpService.GenerateOtpAsync("RESET_PWD", user.Email, TimeSpan.FromMinutes(10), cancellationToken);

        // Add Outbox Event for background processing (e.g. sending email)
        var integrationEvent = new PasswordResetOtpRequestedIntegrationEvent(Guid.NewGuid(), user.Email, otp);
        var payload = JsonSerializer.Serialize(integrationEvent);
        var outboxMessage = new IntegrationOutboxMessage(
            Id: Guid.NewGuid(),
            Type: nameof(PasswordResetOtpRequestedIntegrationEvent),
            Payload: payload,
            OccurredAt: DateTimeOffset.UtcNow);

        await _outboxStore.AddAsync(outboxMessage, cancellationToken);

        return Result.Success();
    }
}
