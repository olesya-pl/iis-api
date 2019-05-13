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

        public TypeEntity(string name, bool isAbstract = false, TypeEntity parent = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (isAbstract && parent != null)
                throw new InvalidOperationException("Only one-tier inheritance is supported.");
            IsAbstract = isAbstract;
            Parent = parent;
        }

        public TypeEntity(string name, IEnumerable<Constraint> constraints, bool isAbstract = false, TypeEntity parent = null)
            : this(name, isAbstract, parent)
        {
            if (constraints == null) throw new ArgumentNullException(nameof(constraints));
            foreach (var constraint in constraints) _constraints.Add(constraint.Name, constraint);
        }

        public Constraint this[string name]
        {
            get => _constraints.GetValueOrDefault(name) ?? Parent?._constraints.GetValueOrDefault(name);
            set => _constraints[name] = value ?? throw new ArgumentNullException(nameof(name));
        }

        public bool HasConstraint(string name) => this[name] != null;

        public void AddConstraint(Constraint constraint)
        {
            if (constraint == null) throw new ArgumentNullException(nameof(constraint));
            _constraints.Add(constraint.Name, constraint);
        }
    }
}
