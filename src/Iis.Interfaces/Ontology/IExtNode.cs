using System;
using System.Collections.Generic;
using System.Text;

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
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        IReadOnlyList<IExtNode> Children { get; }
        bool IsAttribute { get; }
    }
}
