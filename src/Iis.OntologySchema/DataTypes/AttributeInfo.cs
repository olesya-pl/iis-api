using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;

namespace Iis.OntologySchema.DataTypes
{
    public class AttributeInfo : IAttributeInfoList
    {
        public string EntityTypeName { get; set; }
        internal List<AttributeInfoItem> _items = new List<AttributeInfoItem>();
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
    }
}
