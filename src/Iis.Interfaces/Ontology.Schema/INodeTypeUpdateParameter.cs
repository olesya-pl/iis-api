using System;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface INodeTypeUpdateParameter
    {
        Guid? Id { get; }
        string Name { get; }
        EmbeddingOptions? EmbeddingOptions { get; }
        string Meta { get; }
        ScalarType? ScalarType { get; }
        string Title { get; }
        Guid? TargetTypeId { get; }
        Guid? ParentTypeId { get; }
    }
}