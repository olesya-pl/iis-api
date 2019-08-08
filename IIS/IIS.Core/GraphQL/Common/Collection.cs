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
        [GraphQLDeprecated("static collection meta is also deprecated, same as EntityMeta")]
        public CollectionMeta GetMeta() => new CollectionMeta(_source.Count());
        
        [GraphQLNonNullType] //[GraphQLType(typeof(NonNullType<ListType<NonNullType<EntityType>>>))]
        public IEnumerable<TResult> GetItems() => _source.Select(this.Select);

        public int Count => _source.Count();

        protected abstract TResult Select(TSource arg);
    }
}