using System.Collections.Generic;

namespace Iis.Interfaces.Common
{
    public class OutputCollection<T>
    {
        public OutputCollection(IReadOnlyList<T> source)
        : this(source, source.Count)
        { }

        public OutputCollection(IReadOnlyList<T> source, int count)
        {
            Items = source;
            Count = count;
        }

        public IReadOnlyList<T> Items { get; }
        public int Count { get; }
    }
}