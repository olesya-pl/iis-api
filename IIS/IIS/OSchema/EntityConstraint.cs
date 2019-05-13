using System;

namespace IIS.OSchema
{
    public class EntityConstraint : Constraint
    {
        public TypeEntity Target { get; }
        public override TargetKind Kind => TargetKind.Entity;

        public EntityConstraint(string name, TypeEntity target, bool isRequired, bool isArray,
            IRelationResolver resolver = null)
            : base(name, isRequired, isArray, resolver)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
