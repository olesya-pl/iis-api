using System.Collections.Generic;

namespace Iis.Interfaces.OntologyEnum
{
    public interface INodeEnumLibrary
    {
        IEnumerable<string> TypeNames { get; }
        void Add(string typeName, INodeEnumValues nodeEnumValues);
        INodeEnumValues GetEnumValues(string typeName);
    }
}