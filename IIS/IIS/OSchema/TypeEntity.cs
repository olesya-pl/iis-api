using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.OSchema
{
    public class TypeEntity
    {
        private readonly Dictionary<string, Constraint> _constraints = new Dictionary<string, Constraint>();

        public string Name { get; }
        public bool IsAbstract { get; }
        public TypeEntity Parent { get; }
        public IEnumerable<Constraint> OwnConstraints => _constraints.Values;
        public IEnumerable<Constraint> Constraints => Parent == null ? OwnConstraints 
            : OwnConstraints.Concat(Parent.Constraints.Except(OwnConstraints, by => by.Name));
        public IEnumerable<string> ConstraintNames => 
            Parent == null ? _constraints.Keys : _constraints.Keys.Concat(Parent._constraints.Keys).Distinct();

        public TypeEntity(string name, bool isAbstract = false, TypeEntity parent = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (isAbstract && parent != null)
                throw new InvalidOperationException("Only one-tier inheritance is supported.");
            IsAbstract = isAbstract;
            Parent = parent;
        }

        public bool HasConstraint(string name) => GetConstraintOrDefault(name) != null;

        public Constraint GetConstraint(string name) =>
            GetConstraintOrDefault(name) ?? throw new Exception($"Type {Name} does not have constraint {name}.");

        public Constraint AddConstraint(Constraint constraint)
        {
            if (constraint == null) throw new ArgumentNullException(nameof(constraint));
            _constraints.Add(constraint.Name, constraint);
            return constraint;
        }

        private Constraint GetConstraintOrDefault(string name) =>
            _constraints.GetValueOrDefault(name) ?? Parent?._constraints.GetValueOrDefault(name);

    }
}
