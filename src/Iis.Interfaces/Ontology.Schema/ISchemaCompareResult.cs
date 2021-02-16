using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaCompareResult
    {
        IOntologySchema SchemaFrom { get; }
        IOntologySchema SchemaTo { get; }
        IReadOnlyList<INodeTypeLinked> ItemsToAdd { get; }
        IReadOnlyList<INodeTypeLinked> ItemsToDelete { get; }
        IReadOnlyList<ISchemaCompareDiffItem> ItemsToUpdate { get; }
        IReadOnlyList<IAlias> AliasesToAdd { get; }
        IReadOnlyList<IAlias> AliasesToDelete { get; }
        IReadOnlyList<IAlias> AliasesToUpdate { get; }
    }
}
