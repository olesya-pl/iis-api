using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAttributeInfoList
    {
        string EntityTypeName { get; }
        IReadOnlyList<IAttributeInfoItem> Items { get; }
        bool TryAddItem(IAttributeInfoItem item);
    }
}
