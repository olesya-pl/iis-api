using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IChildNodeType
    {
        Guid Id { get; }
        string Name { get; }
        string Title { get; }
        Kind Kind { get; }
        Guid RelationId { get; }
        string RelationName { get; }
        string RelationTitle { get; }
        string RelationMeta { get; }
        string InheritedFrom { get; }
        ScalarType? ScalarType { get; }
        EmbeddingOptions EmbeddingOptions { get; }
        INodeTypeLinked TargetType { get; }
    }
}
