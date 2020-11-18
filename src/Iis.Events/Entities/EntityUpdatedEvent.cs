using MediatR;

namespace Iis.Events.Entities
{
    public class EntityUpdatedEvent : INotification
    {
        public string Type { get; set; }
    }
}
