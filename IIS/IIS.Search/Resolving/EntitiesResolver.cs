using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Search.Ontology;

namespace IIS.Search.Resolving
{
    public class EntitiesResolver : IRelationResolver
    {
        private readonly ISearchService _searchService;

        public EntitiesResolver(ISearchService searchService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var query = context.Parameters.ContainsKey("query") ? (string)context.Parameters["query"] : null;
            var data = await _searchService.SearchAsync(context.RelationName, query);
            var items = new List<object>();
            foreach (var hit in data["hits"]["hits"]) items.Add(hit["_source"]);
            return items;
        }
    }
}
