using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class AttributeRelation : Relation
    {
        private readonly IEnumerable<AttributeValue> _values;
        private readonly AttributeValue _value;

        public new AttributeConstraint Constraint => (AttributeConstraint)base.Constraint;

        public override object Target => Constraint.IsArray ? _values.Select(v => (v.Id, v.Value)) : _value.Value;

        public AttributeRelation(AttributeConstraint constraint, AttributeValue value) : base(constraint)
        {
            if (IsArray) throw new Exception($"Relation {Name} supports arrays only.");
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public AttributeRelation(AttributeConstraint constraint, IEnumerable<AttributeValue> values)
            : base(constraint)
        {
            if (!IsArray) throw new Exception($"Relation {Name} does not support arrays.");
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (!values.Any()) throw new ArgumentException("The list of targets cannot be empty.", nameof(values));
            if (values.AnyDuplicate(v => v.Id)) throw new ArgumentException("Duplicated ids.");
            _values = values;
        }
    }
}
