using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.Comparison
{
    public class SchemaCompareResult : ISchemaCompareResult
    {
        public IReadOnlyList<INodeTypeLinked> ItemsToAdd { get; set; }
        public IReadOnlyList<INodeTypeLinked> ItemsToDelete { get; set; }
        public IReadOnlyList<ISchemaCompareDiffItem> ItemsToUpdate { get; set; }
    }
}
