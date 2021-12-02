using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;

namespace Iis.OntologySchema.DataTypes
{
    public class AttributeInfo : IAttributeInfoList
    {
        internal readonly List<AttributeInfoItem> _items = new List<AttributeInfoItem>();

        public string EntityTypeName { get; set; }
        public IReadOnlyList<IAttributeInfoItem> Items => _items;

        public AttributeInfo(string entityTypeName, IEnumerable<AttributeInfoItem> items)
        {
            EntityTypeName = entityTypeName;
            _items.AddRange(items);
        }

        public bool TryAddItem(IAttributeInfoItem item)
        {
            if(item is null) return false;

            var castedItem = item as AttributeInfoItem;

            if(castedItem is null) return false;

            _items.Add(castedItem);

            return true;
        }

        public void AddItems(IEnumerable<IAttributeInfoItem> items)
        {
            if (items is null) return;

            foreach (var item in items)
            {
                TryAddItem(item);
            }
        }
    }
}