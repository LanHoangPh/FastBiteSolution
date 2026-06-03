namespace FastBiteGroup.Domain.Abstractions.Entities;

public interface IUserTracking
{
    Guid CreatedBy { get; set; }
    Guid? UpdatedBy { get; set; }
}
