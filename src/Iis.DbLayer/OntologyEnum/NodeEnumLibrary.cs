using Iis.Interfaces.OntologyEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumLibrary : INodeEnumLibrary
    {
        Dictionary<string, INodeEnumValues> _dict = new Dictionary<string, INodeEnumValues>();
        public IEnumerable<string> TypeNames => _dict.Keys;
        public INodeEnumValues GetEnumValues(string typeName) => _dict[typeName];
        public void Add(string typeName, INodeEnumValues nodeEnumValues)
        {
            _dict[typeName] = nodeEnumValues;
        }
    }
}
