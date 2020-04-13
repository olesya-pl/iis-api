using System.Collections.Generic;

namespace Iis.Interfaces.OntologyEnum
{
    public interface INodeEnumValue
    {
        IReadOnlyList<INodeEnumValue> Children { get; }
        IEnumerable<string> Keys { get; }
        string GetProperty(string key);
        void AddProperty(string key, string value);
    }
}