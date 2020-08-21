using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class DotNameValues : IDotNameValues
    {
        private List<DotNameValue> _items = new List<DotNameValue>();
        public IReadOnlyList<IDotNameValue> Items => _items;

        public DotNameValues() { }
        public DotNameValues(IEnumerable<DotNameValue> items)
        {
            _items.AddRange(items);
        }
        public bool Contains(string dotName)
        {
            return _items.Any(i => i.DotName == dotName);
        }
        public bool ContainsOneOf(IEnumerable<string> dotNames)
        {
            return dotNames.Any(s => Contains(s));
        }
    }
}
