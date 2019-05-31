using System;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;
using IIS.Core.Ontology;

namespace IIS.Core.Resolving
{
    public class EntityRelationResolver : IRelationResolver
    {
        private readonly IOntology _ontology;
        private readonly IDataLoaderContextAccessor _contextAccessor;

        public EntityRelationResolver(IOntology ontology, IDataLoaderContextAccessor contextAccessor)
        {
            _ontology = ontology ?? throw new ArgumentNullException(nameof(ontology));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var relation = (Relation)context.Source;
            var obj = (Entity)relation.Target;
            var loader = _contextAccessor.Context.GetOrAddBatchLoader<(long, string), IOntologyNode>("GetEntitiesByAsync", _ontology.GetEntitiesByAsync);
            var node = await loader.LoadAsync((obj.Id, context.RelationName));
            if (node is ArrayRelation) return node.Nodes;
            return node;
        }
    }
}
