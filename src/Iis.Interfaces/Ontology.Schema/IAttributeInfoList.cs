using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAttributeInfoList
    {
        string EntityTypeName { get; }
        IReadOnlyList<IAttributeInfoItem> Items { get; }
        bool TryAddItem(IAttributeInfoItem item);
        void AddItems(IEnumerable<IAttributeInfoItem> items);
    }
}