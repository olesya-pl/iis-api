using MediatR;
using System;

namespace Iis.Events.Entities
{
    public class EntityCreatedEvent : INotification
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
    }
}
