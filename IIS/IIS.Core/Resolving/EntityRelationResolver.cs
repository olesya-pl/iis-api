using System;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;

namespace IIS.Core.Resolving
{
    public class EntityRelationResolver : IRelationResolver
    {
        private readonly IOSchema _schema;
        private readonly IDataLoaderContextAccessor _contextAccessor;

        public EntityRelationResolver(IOSchema schema, IDataLoaderContextAccessor contextAccessor)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var entity = (EntityValue)context.Source;
            var loader = _contextAccessor.Context.GetOrAddBatchLoader<long, EntityValue>("GetEntitiesByAsync", _schema.GetEntitiesByAsync);
            entity = await loader.LoadAsync(entity.Value.Id);
            var target = entity.Value.GetRelationTarget(context.RelationName);
            return target;
        }
    }
}
