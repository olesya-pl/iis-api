using Iis.Interfaces.Ontology.Schema;

namespace Iis.DataModel
{
    public class AttributeTypeEntity : BaseEntity, IAttributeType
    {
        public virtual NodeTypeEntity INodeTypeModel { get; set; }
        public ScalarType ScalarType { get; set; }
    }
}
