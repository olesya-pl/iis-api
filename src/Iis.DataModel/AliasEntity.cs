using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DataModel
{
    public class AliasEntity : BaseEntity, IAlias
    {
        public string DotName { get; set; }
        public string Value { get; set; }
        public AliasType Type { get; set; }
    }
}
