using System;

namespace IIS.OSchema
{
    public class AttributeConstraint : Constraint
    {
        public ScalarType Type { get; }
        public override TargetKind Kind => TargetKind.Attribute;

        public AttributeConstraint(string name, ScalarType type, bool isRequired, bool isArray = false, 
            IRelationResolver resolver = null)
            : base(name, isRequired, isArray, resolver)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
