using System.Collections.Generic;
using System.Linq;
using HotChocolate;

namespace IIS.Core.GraphQL.Common
{
    public abstract class Collection<TSource, TResult>
    {
        private readonly IEnumerable<TSource> _source;

        public Collection(IEnumerable<TSource> source)
        {
            _source = source;
        }

        public int Count => _source.Count();

        [GraphQLNonNullType] //[GraphQLType(typeof(NonNullType<ListType<NonNullType<INodeTypeModel>>>))]
        public IEnumerable<TResult> GetItems()
        {
            return _source.Select(Select);
        }

        protected abstract TResult Select(TSource arg);
    }
}
