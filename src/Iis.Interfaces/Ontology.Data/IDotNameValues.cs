using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IDotNameValues
    {
        IReadOnlyList<IDotNameValue> Items { get; }
        bool Contains(string dotName);
        bool ContainsOneOf(IEnumerable<string> dotNames);
        string GetValue(string dotName);
    }
}