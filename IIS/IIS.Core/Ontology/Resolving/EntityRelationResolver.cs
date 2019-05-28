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
            var relation = (Relation)context.Source;
            var obj = (Entity)relation.Target;
            var loader = _contextAccessor.Context.GetOrAddBatchLoader<(long, string), IOntologyNode>("GetEntitiesByAsync", _schema.GetEntitiesByAsync);
            var node = await loader.LoadAsync((obj.Id, context.RelationName));
            if (node is ArrayRelation) return node.Nodes;
            return node;
        }
    }
}
