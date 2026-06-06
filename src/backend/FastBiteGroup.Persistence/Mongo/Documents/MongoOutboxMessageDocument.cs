namespace FastBiteGroup.Persistence.Mongo.Documents;

public sealed class MongoOutboxMessageDocument : MongoDocumentBase<Guid>
{
    public string Type { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public DateTimeOffset OccurredAt { get; init; }

    public string? CorrelationId { get; init; }

    public string Status { get; set; } = MongoOutboxStatuses.Pending;

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public DateTimeOffset? LastAttemptAt { get; set; }
}

public static class MongoOutboxStatuses
{
    public const string Pending = "Pending";
    public const string Processed = "Processed";
    public const string Failed = "Failed";
}
