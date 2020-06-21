using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaAlias: IAlias
    {
        public string DotName { get; set; }
        public string Value { get; set; }
        public string[] Values => Value.Split(',');
        public SchemaAlias() { }
        public SchemaAlias(IAlias alias)
        {
            DotName = alias.DotName;
            Value = alias.Value;
        }
        public SchemaAlias(string dotName, string value)
        {
            DotName = dotName;
            Value = value;
        }
        public override string ToString()
        {
            return $"{DotName}:{Value}";
        }
    }
}
