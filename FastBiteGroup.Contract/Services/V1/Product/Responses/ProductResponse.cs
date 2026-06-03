namespace FastBiteGroup.Contract.Services.V1.Product.Responses;

public sealed record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
