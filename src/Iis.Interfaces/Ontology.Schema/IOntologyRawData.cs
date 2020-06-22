using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IOntologyRawData
    {
        IReadOnlyList<INodeType> NodeTypes { get; }
        IReadOnlyList<IRelationType> RelationTypes { get; }
        IReadOnlyList<IAttributeType> AttributeTypes { get; }
        IReadOnlyList<IAlias> Aliases { get; }
    }
}
