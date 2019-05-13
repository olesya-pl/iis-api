using System;

namespace IIS.OSchema
{
    public abstract class Constraint
    {
        public string Name { get; }
        public bool IsRequired { get; }
        public bool IsArray { get; }
        public IRelationResolver Resolver { get; set; }
        public abstract TargetKind Kind { get; }

        protected Constraint(string name, bool isRequired, bool isArray, IRelationResolver resolver = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsRequired = isRequired;
            IsArray = isArray;
            Resolver = resolver;
        }
    }

    public enum TargetKind
    {
        Attribute = 1,
        Entity,
        Union
    }
}
