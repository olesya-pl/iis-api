using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;
using IIS.Core;
using IIS.Core.Ontology;

namespace IIS.Search.Resolving
{
    public class EntitiesResolver : IRelationResolver
    {
        private readonly OntologySearchService _searchService;
        private readonly IDataLoaderContextAccessor _contextAccessor;

        public EntitiesResolver(OntologySearchService searchService, IDataLoaderContextAccessor contextAccessor)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var data = await _searchService.Find(context.RelationName);
            //var loader = _contextAccessor.Context
            //    .GetOrAddBatchLoader<string, ArrayRelation>("GetEntitiesAsync", _ontology.GetEntitiesAsync);
            //var data = await loader.LoadAsync(context.RelationName);
            //return data.Instances;
            var items = new List<object>();
            foreach (var hit in data["hits"]["hits"]) items.Add(hit["_source"]);
            return items;
        }
    }
}
