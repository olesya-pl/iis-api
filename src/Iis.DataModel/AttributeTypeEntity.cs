using Iis.Interfaces.Ontology.Schema;

namespace Iis.DataModel
{
    public class AttributeTypeEntity : BaseEntity, IAttributeType
    {
        public virtual NodeTypeEntity INodeTypeLinked { get; set; }
        public ScalarType ScalarType { get; set; }

        public string GetDefaultValue()
        {
            throw new System.NotImplementedException();
        }
    }
}
