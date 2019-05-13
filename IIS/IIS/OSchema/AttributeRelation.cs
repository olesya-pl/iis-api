using System;
using System.Collections.Generic;

namespace IIS.OSchema
{
    public class AttributeRelation : Relation
    {
        public new AttributeConstraint Constraint => (AttributeConstraint)base.Constraint;

        public AttributeRelation(AttributeConstraint constraint, AttributeValue value) : base(constraint, value) { }

        public AttributeRelation(AttributeConstraint constraint, IEnumerable<AttributeValue> values)
            : base(constraint, values)
        {
            if (values.HaveSame(v => v.Id)) throw new ArgumentException("Duplicated ids.");
        }
    }

    public class AttributeValue
    {
        public long Id { get; }
        public object Value { get; }

        public AttributeValue(long id, object value)
        {
            Id = id;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
