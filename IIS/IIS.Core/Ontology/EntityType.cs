using System;
using System.Linq;

namespace IIS.Core.Ontology
{
    public sealed class EntityType : Type
    {
        public bool IsAbstract { get; }
        public bool IsMarker => !RelatedTypes.Any();

        public override System.Type ClrType => typeof(Entity);

        public EntityType(Guid id, string name, bool isAbstract)
            : base(id, name)
        {
            IsAbstract = isAbstract;
        }
    }
}
