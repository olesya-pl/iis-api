using System;

namespace Iis.Events.Entities
{
    public abstract class EntityEvent
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
    }
}