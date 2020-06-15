using Iis.Interfaces.OntologyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumValue : INodeEnumValue
    {
        Dictionary<string, string> _dict = new Dictionary<string, string>();
        List<NodeEnumValue> _children = new List<NodeEnumValue>();

        public IEnumerable<string> Keys => _dict.Keys;
        public string GetProperty(string key) => _dict[key];
        public IReadOnlyList<INodeEnumValue> Children => _children;
        public void AddProperty(string key, string value)
        {
            _dict[key] = value;
        }
    }
}
