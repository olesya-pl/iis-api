using Iis.Interfaces.AccessLevels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData.IisAccessLevels
{
    public class AccessLevels : IAccessLevels
    {
        private List<AccessLevel> _items;
        public IReadOnlyList<AccessLevel> Items => _items;
        public bool IndexIsValid(int numericIndex) =>
            Items.Any(item => item.NumericIndex == numericIndex);

        public AccessLevels(IEnumerable<AccessLevel> items)
        {
            _items = items.OrderBy(x => x.NumericIndex).ToList();
        }
    }
}
