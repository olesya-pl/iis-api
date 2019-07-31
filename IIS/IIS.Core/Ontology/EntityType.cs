using System;
using System.Linq;

namespace IIS.Core.Ontology
{
    public class EntityType : Type
    {
        public bool IsAbstract { get; }
        public bool IsMarker => !Nodes.Any();

        public EntityType(Guid id, string name, bool isAbstract)
            : base(id, name)
        {
            IsAbstract = isAbstract;
        }
    }
}
