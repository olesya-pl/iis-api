using MediatR;
using System;

namespace Iis.Events.Entities
{
    public class EntityUpdatedEvent : EntityEvent, INotification
    {
        public Guid RequestId { get; set; }
    }
}
