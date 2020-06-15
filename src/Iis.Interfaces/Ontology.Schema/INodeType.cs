using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface INodeType
    {
        Guid Id { get; }
        string Name { get; }
        string Title { get; }
        string Meta { get; }
        string Aliases { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        bool IsArchived { get; }
        Kind Kind { get; }
        bool IsAbstract { get; }
    }
}
