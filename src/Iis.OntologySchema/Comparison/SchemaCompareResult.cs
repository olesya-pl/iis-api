using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.Comparison
{
    public class SchemaCompareResult : ISchemaCompareResult
    {
        public IOntologySchemaSource SchemaSource { get; set; }
        public IReadOnlyList<INodeTypeLinked> ItemsToAdd { get; set; }
        public IReadOnlyList<INodeTypeLinked> ItemsToDelete { get; set; }
        public IReadOnlyList<ISchemaCompareDiffItem> ItemsToUpdate { get; set; }
        public IReadOnlyList<IAlias> AliasesToAdd { get; set; }
        public IReadOnlyList<IAlias> AliasesToDelete { get; set; }
        public IReadOnlyList<IAlias> AliasesToUpdate { get; set; }
    }
}
