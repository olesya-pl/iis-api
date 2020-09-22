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
        public IReadOnlyList<INode> Nodes { get; set; }
        public DotNameValue() { }
        public DotNameValue(string dotName, string value, IEnumerable<INode> nodes)
        {
            DotName = dotName;
            Value = value;
            Nodes = new List<INode>(nodes);
        }
        public override string ToString()
        {
            return $"{DotName}: {Value}";
        }
    }
}