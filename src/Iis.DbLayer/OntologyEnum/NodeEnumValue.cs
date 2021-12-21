using System.Collections.Generic;
using Iis.Interfaces.OntologyEnum;

namespace Iis.DbLayer.OntologyEnum
{
    public class NodeEnumValue : INodeEnumValue
    {
        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();
        private readonly List<NodeEnumValue> _children = new List<NodeEnumValue>();

        public IEnumerable<string> Keys => _dict.Keys;
        public IReadOnlyList<INodeEnumValue> Children => _children;
        public string GetProperty(string key) => _dict[key];
        public void AddProperty(string key, string value)
        {
            _dict[key] = value;
        }
    }
}
