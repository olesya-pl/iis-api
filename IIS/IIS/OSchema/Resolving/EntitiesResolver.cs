using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;

namespace IIS.OSchema.Resolving
{
    public class EntitiesResolver : IRelationResolver
    {
        private readonly IOSchema _schema;
        private readonly IDataLoaderContextAccessor _contextAccessor;

        public EntitiesResolver(IOSchema schema, IDataLoaderContextAccessor contextAccessor)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var relationName = context.RelationName.Camelize();
            var loader = _contextAccessor.Context
                .GetOrAddBatchLoader<string, IEnumerable<EntityValue>>("GetEntitiesAsync", _schema.GetEntitiesAsync);
            var data = await loader.LoadAsync(relationName);
            return data;
        }
    }
}
