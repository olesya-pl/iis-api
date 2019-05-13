using System;
using System.Collections.Generic;

namespace IIS.OSchema
{
    public class EntityRelation : Relation
    {
        public new EntityConstraint Constraint => (EntityConstraint)base.Constraint;

        public EntityRelation(EntityConstraint constraint, Entity entity) : base(constraint, entity) { }

        public EntityRelation(EntityConstraint constraint, IEnumerable<Entity> entities)
            : base(constraint, entities)
        {
            if (entities.HaveSame(v => v.Id)) throw new ArgumentException("Duplicated ids.");
            if (!entities.HaveAllSame(v => v.Type.Parent.Name))
                throw new ArgumentException("Different entity types within the same set.");
        }
    }
}
