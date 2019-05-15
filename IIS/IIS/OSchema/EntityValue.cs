using System;
using System.Collections.Generic;

namespace IIS.OSchema
{
    public class AttributeValue
    {
        public long Id { get; }
        public object Value { get; }

        public AttributeValue(long id, object value)
        {
            Id = id;
            Value = value;
        }
    }

    public class EntityValue
    {
        public Entity Value { get; }
        public IEnumerable<AttributeRelation> RelationInfo { get; }

        public EntityValue(Entity value, IEnumerable<AttributeRelation> relationInfo)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            RelationInfo = relationInfo ?? throw new ArgumentNullException(nameof(relationInfo));
        }
    }
}
