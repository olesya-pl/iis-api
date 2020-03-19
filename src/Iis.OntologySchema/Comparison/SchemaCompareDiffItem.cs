using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.Comparison
{
    public class SchemaCompareDiffItem : ISchemaCompareDiffItem
    {
        public INodeTypeLinked NodeTypeFrom { get; set; }
        public INodeTypeLinked NodeTypeTo { get; set; }
    }
}
