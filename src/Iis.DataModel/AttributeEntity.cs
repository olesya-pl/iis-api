using Iis.Interfaces.Ontology.Data;

namespace Iis.DataModel
{
    public class AttributeEntity : BaseEntity, IAttributeBase
    {
        public virtual NodeEntity Node { get; set; }
        public string Value { get; set; }
    }
}
