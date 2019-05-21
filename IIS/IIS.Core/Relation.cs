using System;

namespace IIS.Core
{
    public abstract class Relation
    {
        public abstract object Target { get; }

        public Constraint Constraint { get; }

        public string Name => Constraint.Name;
        public TargetKind TargetKind => Constraint.Kind;
        public bool IsArray => Constraint.IsArray;

        protected Relation(Constraint constraint)
        {
            Constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
        }
    }
}
