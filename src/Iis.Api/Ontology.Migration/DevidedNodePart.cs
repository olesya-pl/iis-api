using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class DevidedNodePart
    {
        public string TypeName { get; private set; }
        public string Value { get; private set; }

        public DevidedNodePart() { }
        public DevidedNodePart(string typeName, string value)
        {
            TypeName = typeName;
            Value = value;
        }
    }
}
