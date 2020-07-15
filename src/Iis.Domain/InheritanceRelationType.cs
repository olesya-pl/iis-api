using System;
using System.Linq;

namespace Iis.Domain
{
    public sealed class InheritanceRelationType : RelationType, IInheritanceRelationTypeModel
    {
        public static readonly string RelationName = "Is";

        public IEntityTypeModel ParentType => RelatedTypes.OfType<IEntityTypeModel>().Single(); // Inheritance relation should always have single IEntityTypeModel node (parent)

        public override Type ClrType =>
            throw new NotSupportedException("Inheritance type does not have Clr type");

        public InheritanceRelationType(Guid id)
            : base(id, RelationName)
        {
        }
    }
}
