using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.Comparison
{
    public class SchemaCompareDiffItem : ISchemaCompareDiffItem
    {
        public INodeTypeLinked NewNode { get; set; }
        public INodeTypeLinked OldNode { get; set; }
    }
}
