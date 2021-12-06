using System.Collections.Generic;

namespace Iis.Domain.Materials
{
    public class MaterialCollection
    {
        public MaterialCollection(IReadOnlyCollection<Material> collection, int count)
        {
            Items = collection;
            Count = count;
        }

        public IReadOnlyCollection<Material> Items { get; }
        public int Count { get; }
    }
}