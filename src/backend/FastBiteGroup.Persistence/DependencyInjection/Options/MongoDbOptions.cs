using System.ComponentModel.DataAnnotations;

namespace FastBiteGroup.Persistence.DependencyInjection.Options;

public sealed class MongoDbOptions
{

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = "fastbite";

    [Required]
    public string OutboxCollectionName { get; set; } = "integration_outbox";

    [Required]
    public string MessagesCollectionName { get; set; } = "messages";

    [Required]
    public string NotificationsCollectionName { get; set; } = "notifications";
}
