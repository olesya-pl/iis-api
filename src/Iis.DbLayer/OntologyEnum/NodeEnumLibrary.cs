using System.Collections.Generic;
using Iis.Interfaces.OntologyEnum;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumLibrary : INodeEnumLibrary
    {
        private readonly Dictionary<string, INodeEnumValues> _dict = new Dictionary<string, INodeEnumValues>();
        public IEnumerable<string> TypeNames => _dict.Keys;
        public INodeEnumValues GetEnumValues(string typeName) => _dict[typeName];
        public void Add(string typeName, INodeEnumValues nodeEnumValues)
        {
            _dict[typeName] = nodeEnumValues;
        }
    }
}
