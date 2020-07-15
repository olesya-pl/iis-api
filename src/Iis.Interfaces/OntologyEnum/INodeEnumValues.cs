using System.Collections.Generic;

namespace Iis.Interfaces.OntologyEnum
{
    public interface INodeEnumValues
    {
        IReadOnlyList<INodeEnumValue> Items { get; }
    }
}