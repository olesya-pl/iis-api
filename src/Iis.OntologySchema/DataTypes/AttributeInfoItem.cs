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
        public AttributeInfoItem(string dotName, ScalarType scalarType)
        {
            DotName = dotName;
            ScalarType = scalarType;
        }
    }
}
