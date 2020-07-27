using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class AttributeInfoItem : IAttributeInfoItem
    {
        public string DotName { get; set; }
        public ScalarType ScalarType { get; set; }
        public IEnumerable<string> AliasesList { get; set; }
        public AttributeInfoItem(string dotName, ScalarType scalarType, IEnumerable<string> aliasesList)
        {
            DotName = dotName;
            ScalarType = scalarType;
            AliasesList = aliasesList ?? new List<string>();
        }
        public override string ToString()
        {
            return DotName;
        }
    }
}
