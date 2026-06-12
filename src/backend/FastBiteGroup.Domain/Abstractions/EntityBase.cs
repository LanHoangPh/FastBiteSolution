namespace FastBiteGroup.Domain.Abstractions;

public abstract class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; } = default!;
}
