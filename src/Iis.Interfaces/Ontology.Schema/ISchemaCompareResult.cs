using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaCompareResult
    {
        IReadOnlyList<INodeTypeLinked> ItemsToAdd { get; }
        IReadOnlyList<INodeTypeLinked> ItemsToDelete { get; }
        IReadOnlyList<INodeTypeLinked> ItemsToUpdate { get; }
    }
}
