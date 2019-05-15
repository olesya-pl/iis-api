using System;
using System.Collections.Generic;

namespace IIS.OSchema
{
    public class UnionRelation : Relation
    {
        private readonly IEnumerable<EntityValue> _values;

        public new UnionConstraint Constraint => (UnionConstraint)base.Constraint;

        public override object Target => _values;

        public UnionRelation(UnionConstraint constraint, IEnumerable<EntityValue> entities) : base(constraint) {
            _values = entities ?? throw new ArgumentNullException(nameof(entities));
        }
    }
}
