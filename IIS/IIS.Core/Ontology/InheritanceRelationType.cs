using System;
using System.Linq;

namespace IIS.Core.Ontology
{
    public class InheritanceRelationType : RelationType
    {
        public static readonly string RelationName = "Is";
        public EntityType ParentType => RelatedTypes.OfType<EntityType>().Single(); // Inheritance relation should always have single EntityType node (parent)

        public override System.Type ClrType =>
            throw new NotSupportedException("Inheritance type does not have Clr type");

        public InheritanceRelationType(Guid id)
            : base(id, RelationName)
        {

        }
    }
}
