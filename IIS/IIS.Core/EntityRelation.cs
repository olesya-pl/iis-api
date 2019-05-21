using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class EntityRelation : Relation
    {
        private readonly IEnumerable<EntityValue> _values;
        private readonly EntityValue _value;

        public new EntityConstraint Constraint => (EntityConstraint)base.Constraint;

        public override object Target => Constraint.IsArray ? (object)_values : _value;

        public EntityRelation(EntityConstraint constraint, EntityValue entity) : base(constraint)
        {
            if (IsArray) throw new Exception($"Relation {Name} supports arrays only.");
            _value = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public EntityRelation(EntityConstraint constraint, IEnumerable<EntityValue> entities)
            : base(constraint)
        {
            if (!IsArray) throw new Exception($"Relation {Name} does not support arrays.");
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            if (entities.AnyDuplicate(v => v.Value.Id)) throw new ArgumentException("Duplicated ids.");
            if (!entities.HaveSame(v => v.Value.Type.Parent?.Name))
                throw new ArgumentException("Different entity types within the same set.");
            _values = entities;
        }
    }
}
