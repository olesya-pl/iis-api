using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;
using IIS.Core.Ontology;

namespace IIS.Core.Resolving
{
    public class EntitiesResolver : IRelationResolver
    {
        private readonly IOntology _ontology;
        private readonly IDataLoaderContextAccessor _contextAccessor;

        public EntitiesResolver(IOntology ontology, IDataLoaderContextAccessor contextAccessor)
        {
            _ontology = ontology ?? throw new ArgumentNullException(nameof(ontology));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var loader = _contextAccessor.Context
                .GetOrAddBatchLoader<string, ArrayRelation>("GetEntitiesAsync", _ontology.GetEntitiesAsync);
            var data = await loader.LoadAsync(context.RelationName);
            return data.Instances;
        }
    }
}
