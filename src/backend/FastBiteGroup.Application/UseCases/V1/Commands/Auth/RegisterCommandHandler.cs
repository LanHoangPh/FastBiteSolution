using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Application.Abstractions.Caching;
using System.Text.Json;
using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Abstractions.Outbox;
using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Events;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RegisterCommandHandler
    : ICommandHandler<AuthCommands.RegisterCommand, RegisterResponse>
{
    private readonly IUserAuthService _userAuthService;
    private readonly ICacheService _cacheService;
    private readonly IIntegrationOutboxStore _outboxStore;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserAuthService userAuthService,
        ICacheService cacheService,
        IIntegrationOutboxStore outboxStore,
        ILogger<RegisterCommandHandler>? logger = null)
    {
        _userAuthService = userAuthService;
        _cacheService = cacheService;
        _outboxStore = outboxStore;
        _logger = logger ?? NullLogger<RegisterCommandHandler>.Instance;
    }

    public async Task<Result<RegisterResponse>> Handle(
        AuthCommands.RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if email already exists
        var existing = await _userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            _logger.LogWarning("Registration failed: email already exists.");
            return Result.Failure<RegisterResponse>(
                new Error("Auth.EmailAlreadyExists", $"Email '{request.Email}' is already registered."));
        }

        // 2. Create user (inactive, email not confirmed)
        var (user, errorMessage) = await _userAuthService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName,
            request.DayOfBirth, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Registration failed while creating user. Error: {Error}", errorMessage);
            return Result.Failure<RegisterResponse>(new Error("Auth.RegistrationFailed", errorMessage!));
        }

        // 3. Generate 6-digit OTP
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = $"OTP_REG_{user.Email.ToUpperInvariant()}";
        await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(10), cancellationToken);

        // 4. Generate Magic Link Token
        var magicLinkToken = await _userAuthService.GenerateEmailConfirmationTokenAsync(user.Email, cancellationToken);

        // 5. Publish to Outbox
        var integrationEvent = new UserRegisteredIntegrationEvent(
            user.Id,
            user.Email,
            otp,
            magicLinkToken);

        var outboxMessage = new IntegrationOutboxMessage(
            Guid.NewGuid(),
            nameof(UserRegisteredIntegrationEvent),
            JsonSerializer.Serialize(integrationEvent),
            DateTimeOffset.UtcNow);

        await _outboxStore.AddAsync(outboxMessage, cancellationToken);

        _logger.LogInformation(
            "Registration outbox message saved for {Email}. OTP: {Otp}",
            user.Email, otp);

        return Result.Success(new RegisterResponse("User registered successfully. Please check your email to activate the account."));
    }
}
