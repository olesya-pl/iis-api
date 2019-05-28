using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class TypeEntity : IType
    {
        private readonly Dictionary<string, Constraint> _ownConstraints = new Dictionary<string, Constraint>();

        // IType
        public string Name { get; }
        public Kind Kind => Kind.Class;

        public bool IsAbstract { get; }
        public TypeEntity Parent { get; }

        public IEnumerable<Constraint> OwnConstraints => _ownConstraints.Values;
        public IEnumerable<Constraint> Constraints
        {
            get
            {
                if (!HasParent) return OwnConstraints;
                var parentConstraints = Parent.Constraints.Except(OwnConstraints, by => by.Name);
                return OwnConstraints.Concat(parentConstraints);
            }
        }
        public IEnumerable<string> ConstraintNames => Constraints.Select(c => c.Name);

        public TypeEntity(string name, bool isAbstract = false, TypeEntity parent = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (parent != null && (parent.HasParent || !parent.IsAbstract || IsAbstract)) 
                throw new InvalidInheritanceException(name, parent.Name);
            IsAbstract = isAbstract;
            Parent = parent;
        }

        public bool HasParent => Parent != null;

        public bool HasConstraint(string constraintName) => ConstraintNames.Contains(constraintName);

        public Constraint GetConstraint(string constraintName)
        {
            if (!HasConstraint(constraintName)) throw new ConstraintNotFoundException(Name, constraintName);
            return _ownConstraints.GetValueOrDefault(constraintName) ?? Parent._ownConstraints[constraintName];
        }

        public Constraint AddConstraint(Constraint constraint)
        {
            if (constraint == null) throw new ArgumentNullException(nameof(constraint));
            _ownConstraints.Add(constraint.Name, constraint);
            return constraint;
        }

        public Constraint AddType(string constraintName, IType type, bool isRequired, bool isArray = false, 
            IRelationResolver resolver = null)
        {
            var constraint = new Constraint(constraintName, type, isRequired, isArray, resolver);
            _ownConstraints.Add(constraint.Name, constraint);
            return constraint;
        }

        // ISchemaNode
        public void AcceptVisitor(ISchemaVisitor visitor)
        {
            if (IsAbstract) visitor.VisitAbstractClass(this);
            else visitor.VisitClass(this);
        }
        IEnumerable<ISchemaNode> ISchemaNode.Nodes => Constraints;
    }
}
