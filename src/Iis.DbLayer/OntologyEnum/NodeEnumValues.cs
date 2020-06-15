using Iis.Interfaces.OntologyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumValues : INodeEnumValues
    {
        List<INodeEnumValue> _items = new List<INodeEnumValue>();
        public IReadOnlyList<INodeEnumValue> Items => _items;
        public NodeEnumValues(IEnumerable<INodeEnumValue> enumValueList)
        {
            _items = enumValueList.ToList();
        }
    }
}
