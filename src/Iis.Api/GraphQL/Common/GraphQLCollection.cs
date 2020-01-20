using HotChocolate;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.Common
{
    public class GraphQLCollection<T> : IEnumerable<T>
    {
        private IList<T> _items = new List<T>();

        public GraphQLCollection() { }
        public GraphQLCollection(IList<T> list, int totalRecordsCount)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            _items = list;
            Count = totalRecordsCount;
        }
        public void Add(T element) => _items.Add(element);
        public int Count { get; set; }
        [GraphQLNonNullType]
        public IEnumerable<T> GetItems() => _items;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}
