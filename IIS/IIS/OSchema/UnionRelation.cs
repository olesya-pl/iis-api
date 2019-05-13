using System.Collections.Generic;

namespace IIS.OSchema
{
    public class UnionRelation : Relation
    {
        public new UnionConstraint Constraint => (UnionConstraint)base.Constraint;

        public UnionRelation(UnionConstraint constraint, IEnumerable<Entity> entities) : base(constraint, entities) { }
    }
}
