using FastBiteGroup.Contract.Services.V1.Auth;
using FastBiteGroup.Contract.Services.V1.Auth.Commands;
using FastBiteGroup.Contract.Services.V1.Auth.Events;
using FastBiteGroup.Contract.Services.V1.Auth.Responses;
using System.Text.Json;

namespace FastBiteGroup.Application.UseCases.V1.Commands.Auth;

internal sealed class RegisterCommandHandler(
    IUserAuthService userAuthService,
    IIntegrationOutboxStore outboxStore,
    ILogger<RegisterCommandHandler>? logger = null)
    : ICommandHandler<AuthCommands.RegisterCommand, RegisterResponse>
{
    private readonly ILogger<RegisterCommandHandler> _logger = logger ?? NullLogger<RegisterCommandHandler>.Instance;

    public async Task<Result<RegisterResponse>> Handle(
        AuthCommands.RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await userAuthService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            _logger.LogWarning("Registration failed: email already exists.");
            return Result.Failure<RegisterResponse>(
                AuthErrors.EmailAlreadyExists(request.Email));
        }

        // Create user (inactive, email not confirmed)
        var (user, errorMessage) = await userAuthService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName,
            request.DayOfBirth, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Registration failed while creating user. Error: {Error}", errorMessage);
            return Result.Failure<RegisterResponse>(AuthErrors.RegistrationFailed(errorMessage!));
        }

        var emailConfirmationToken = await userAuthService.GenerateEmailConfirmationTokenAsync(user.Email, cancellationToken);

        //Publish to Outbox
        var integrationEvent = new UserRegisteredIntegrationEvent(
            user.Id,
            user.Email,
            emailConfirmationToken);

        var outboxMessage = new IntegrationOutboxMessage(
            Guid.NewGuid(),
            nameof(UserRegisteredIntegrationEvent),
            JsonSerializer.Serialize(integrationEvent),
            DateTimeOffset.UtcNow);

        await outboxStore.AddAsync(outboxMessage, cancellationToken);

        _logger.LogInformation("Registration outbox message saved for {Email}.", user.Email);

        return Result.Success(new RegisterResponse("User registered successfully. Please check your email to activate the account."));
    }
}
