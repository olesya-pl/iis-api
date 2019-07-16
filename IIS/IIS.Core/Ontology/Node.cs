using System;

namespace IIS.Core.Ontology
{
    public abstract class Node
    {
        public virtual Guid Id { get; }
        public virtual Type Type { get; }
        public virtual Node[] Nodes { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }
    }

    public class Attribute : Node
    {
        public virtual object Value { get; }
    }

    public class Entity : Node
    {

    }

    public class Relation : Node
    {

    }
}
