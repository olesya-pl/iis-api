using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.OSchema
{
    public abstract class Relation
    {
        private readonly Dictionary<string, AttributeValue> _attributes = new Dictionary<string, AttributeValue>();
        private readonly object _target;

        //public RelationInfo RelationInfo { get; }
        public Constraint Constraint { get; }

        public string Name => Constraint.Name;
        public TargetKind TargetKind => Constraint.Kind;
        public bool IsArray => Constraint.IsArray;

        protected Relation(Constraint constraint, object target)
        {
            Constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        protected Relation(Constraint constraint, IEnumerable<object> targets)
            : this(constraint, (object)targets)
        {
            if (!IsArray) throw new Exception($"Relation {Name} supports arrays only.");
            if (!targets.Any()) throw new ArgumentException("The list of targets cannot be empty.", nameof(targets));
        }

        public object GetSingularTarget() => !IsArray ? _target
            : throw new Exception($"Relation {Name} supports arrays only.");

        public IEnumerable<object> GetPluralTarget() => IsArray ? (IEnumerable<object>)_target 
            : throw new Exception($"Relation {Name} does not support arrays.");
    }

    //public class RelationInfo
    //{
    //    public IEnumerable<AttributeRelation> Attributes { get; set; }
    //}
}
