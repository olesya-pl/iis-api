using System;
using System.Collections.Generic;

namespace IIS.OSchema
{
    public class UnionConstraint : Constraint
    {
        private readonly Dictionary<string, TypeEntity> _targets = new Dictionary<string, TypeEntity>();

        public IEnumerable<TypeEntity> Targets => _targets.Values;
        public override TargetKind Kind => TargetKind.Union;

        public UnionConstraint(string name, bool isRequired, bool isArray = false, IRelationResolver resolver = null)
             : base(name, isRequired, isArray, resolver) { }

        public UnionConstraint(string name, IEnumerable<TypeEntity> types, bool isRequired, bool isArray = false,
            IRelationResolver resolver = null)
            : this(name, isRequired, isArray, resolver)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            foreach (var type in types) _targets.Add(type.Name, type);
        }

        public TypeEntity this[string name]
        {
            get => _targets.GetValueOrDefault(name);
            set => _targets[name] = value ?? throw new ArgumentNullException(nameof(name));
        }

        public void AddType(TypeEntity type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            _targets.Add(type.Name, type);
        }
    }
}
