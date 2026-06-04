namespace FastBiteGroup.Contract.Abstractions.Outbox;

public sealed record IntegrationOutboxMessage(
    Guid Id,
    string Type,
    string Payload,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null);
