using System.Collections.Generic;
using System.Linq;
using Iis.Interfaces.OntologyEnum;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumValues : INodeEnumValues
    {
        private readonly List<INodeEnumValue> _items = new List<INodeEnumValue>();
        public NodeEnumValues(IEnumerable<INodeEnumValue> enumValueList)
        {
            _items = enumValueList.ToList();
        }
        public IReadOnlyList<INodeEnumValue> Items => _items;
    }
}
