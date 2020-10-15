using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;

namespace Iis.OntologySchema.DataTypes
{
    public class AttributeInfoItem : IAttributeInfoItem
    {
        public string DotName { get; private set; }
        public ScalarType ScalarType { get; private set; }
        public IEnumerable<string> AliasesList { get; private set; }
        public bool IsAggregated { get; private set; }

        public AttributeInfoItem(string dotName, ScalarType scalarType, IEnumerable<string> aliasesList, bool isAggregated)
        {
            DotName = dotName;
            ScalarType = scalarType;
            AliasesList = aliasesList ?? new List<string>();
            IsAggregated = isAggregated;
        }
        public override string ToString()
        {
            return DotName;
        }
    }
}
