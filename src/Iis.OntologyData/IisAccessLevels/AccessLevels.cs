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
        public List<AccessLevel> Items => _items;
        public bool IndexIsValid(int numericIndex) =>
            Items.Any(item => item.NumericIndex == numericIndex);

        public AccessLevels(IEnumerable<AccessLevel> items)
        {
            _items = items.OrderBy(x => x.NumericIndex).ToList();
        }

        public AccessLevel GetItemById(Guid id) =>
            _items.FirstOrDefault(x => x.Id == id);

        public AccessLevel GetItemByNumericIndex(int numericIndex) =>
            _items.FirstOrDefault(x => x.NumericIndex == numericIndex);

        public Dictionary<int, int> GetChangedIndexes(IAccessLevels anotherAccessLevels)
        {
            var dict = new Dictionary<int, int>();
            foreach (var item in Items)
            {
                var anotherItem = anotherAccessLevels.GetItemById(item.Id);
                if (anotherItem != null || anotherItem.NumericIndex != item.NumericIndex)
                {
                    dict[item.NumericIndex] = anotherItem.NumericIndex;
                }
            }

            return dict;
        }
    }
}
