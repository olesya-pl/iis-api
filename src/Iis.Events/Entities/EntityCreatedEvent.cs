using MediatR;

namespace Iis.Events.Entities
{
    public class EntityCreatedEvent : INotification
    {
        public string Type { get; set; }
    }
}
