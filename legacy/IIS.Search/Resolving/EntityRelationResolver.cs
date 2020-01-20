using System;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;
using Newtonsoft.Json.Linq;

namespace IIS.Search.Resolving
{
    public class EntityRelationResolver : IRelationResolver
    {
        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var src = (JObject)context.Source;
            var prop = src[context.RelationName];
            //var obj = (Entity)relation.Target;
            //var loader = _contextAccessor.Context.GetOrAddBatchLoader<(long, string), IOntologyNode>("GetEntitiesByAsync", _ontology.GetEntitiesByAsync);
            //var node = await loader.LoadAsync((obj.Id, context.RelationName));
            //if (node is ArrayRelation) return node.Nodes;
            //return node;
            return prop;
        }
    }
}
