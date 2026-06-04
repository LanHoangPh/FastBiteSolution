using MediatR;

namespace FastBiteGroup.Contract.Abstractions.Message;

public interface IDomainEvent : INotification
{
    public Guid IdEvent { get; }
    DateTime OccurredOn { get; } // dùng cho audit log, tracking, hoặc để đảm bảo thứ tự của các sự kiện
}
