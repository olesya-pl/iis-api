using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class DotNameValue : IDotNameValue
    {
        public string DotName { get; set; }
        public string Value { get; set; }
        public DotNameValue() { }
        public DotNameValue(string dotName, string value)
        {
            DotName = dotName;
            Value = value;
        }
        public override string ToString()
        {
            return $"{DotName}: {Value}";
        }
    }
}
