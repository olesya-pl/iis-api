using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Ontology
{
    public interface IExtNode
    {
        string Id { get; }
        string NodeTypeId { get; }
        string NodeTypeName { get; }
        string NodeTypeTitle { get; }
        string EntityTypeName { get; }
        object AttributeValue { get; }
        ScalarType? ScalarType { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        IReadOnlyList<IExtNode> Children { get; }
        bool IsAttribute { get; }
        List<IGeoCoordinates> GetCoordinates();
        List<(IExtNode Node, IGeoCoordinates Coordinates)> GetNodeCoordinates();
        List<IExtNode> GetAttributesRecursive(ScalarType scalarType);
    }
}
