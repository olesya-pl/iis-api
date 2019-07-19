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
        
        [GraphQLNonNullType]
        public CollectionMeta GetMeta() => new CollectionMeta(_source.Count());
        
        [GraphQLNonNullType] //[GraphQLType(typeof(NonNullType<ListType<NonNullType<EntityType>>>))]
        public IEnumerable<TResult> GetItems() => _source.Select(this.Select);

        protected abstract TResult Select(TSource arg);
    }
}