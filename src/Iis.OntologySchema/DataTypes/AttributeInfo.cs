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
    }
}
