using System;
using MediatR;

namespace Iis.Events.Entities
{
    public class EntityDeleteEvent : EntityEvent, INotification
    {
        private EntityDeleteEvent(){}

        public static EntityDeleteEvent Create(Guid entityId, string entityType)
        {
            return new EntityDeleteEvent
            {
                Id = entityId,
                Type = entityType
            };
        }
    }
}
