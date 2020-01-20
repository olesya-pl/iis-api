using System;
using System.Linq;

namespace Iis.Domain
{
    public sealed class EntityType : NodeType
    {
        public bool IsAbstract { get; }

        public bool IsMarker => !RelatedTypes.Any();

        public override Type ClrType => typeof(Entity);

        public EntityType(Guid id, string name, bool isAbstract)
            : base(id, name)
        {
            IsAbstract = isAbstract;
        }
    }
}
