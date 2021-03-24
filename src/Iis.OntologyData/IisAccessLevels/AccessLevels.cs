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
        public IReadOnlyList<IAccessLevel> Items => _items;
        public List<AccessLevel> OriginalItems => _items;
        public bool IndexIsValid(int numericIndex) =>
            Items.Any(item => item.NumericIndex == numericIndex);

        public AccessLevels(IEnumerable<IAccessLevel> items)
        {
            _items = items
                .OrderBy(x => x.NumericIndex)
                .Select(x => new AccessLevel(x))
                .ToList();
        }

        public IAccessLevel GetItemById(Guid id) =>
            _items.FirstOrDefault(x => x.Id == id);

        public IAccessLevel GetItemByNumericIndex(int numericIndex) =>
            _items.FirstOrDefault(x => x.NumericIndex == numericIndex);

        public void Add(AccessLevel accessLevel)
        { 
            _items.Add(accessLevel); 
            Reindex(); 
        }

        public void ChangeName(Guid id, string name) =>
            _items.Single(x => x.Id == id).Name = name;

        public void SwapItems(int index1, int index2)
        {
            var temp = _items[index1];
            _items[index1] = _items[index2];
            _items[index2] = temp;
            Reindex();
        }

        private void Reindex()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].NumericIndex = i;
            }
        }

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

        public void Remove(AccessLevel accessLevel)
        {
            _items.Remove(accessLevel);
            Reindex();
        }
    }
}
