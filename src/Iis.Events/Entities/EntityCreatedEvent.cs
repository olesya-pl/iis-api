using MediatR;

namespace Iis.Events.Entities
{
    public class EntityCreatedEvent : EntityEvent, INotification
    {
    }
}
