using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IRelation: IRelationBase
    {
        INode Node { get; }
        INode SourceNode { get; }
        INode TargetNode { get; }

        Kind TargetKind { get; }
        RelationKind RelationKind { get; }
        EmbeddingOptions EmbeddingOptions { get; }
        string RelationTypeName { get; }
        bool IsLinkToSeparateObject { get; }
        bool IsLinkToExternalObject { get; }
        bool IsLinkToAttribute { get; }
        bool IsLinkToInternalObject { get; }
        string TypeName { get; }
    }
}
