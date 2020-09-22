using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IDotNameValue
    {
        string DotName { get; }
        string Value { get; }
        IReadOnlyList<INode> Nodes { get; }
    }
}