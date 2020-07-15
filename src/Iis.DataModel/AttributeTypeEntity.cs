using Iis.Interfaces.Ontology.Schema;

namespace Iis.DataModel
{
    public class AttributeTypeEntity : BaseEntity, IAttributeType
    {
        public virtual NodeTypeEntity NodeType { get; set; }
        public ScalarType ScalarType { get; set; }
    }
}
