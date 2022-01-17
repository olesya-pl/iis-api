using System;
using MediatR;

namespace Iis.Events.Entities
{
    public class EntityUpdatedEvent : EntityEvent, INotification
    {
        public Guid RequestId { get; set; }
    }
}
