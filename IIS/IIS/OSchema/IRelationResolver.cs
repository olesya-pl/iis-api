using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.OSchema
{
    public interface IRelationResolver
    {
        Task<object> ResolveAsync(ResolveContext context);
    }

    public class ResolveContext
    {
        public string RelationName { get; set; }

        public IDictionary<string, object> Parameters { get; set; }
    }

    public class EntitiesResolver : IRelationResolver
    {
        private readonly IOSchema _oSchema;

        public EntitiesResolver(IOSchema oSchema)
        {
            _oSchema = oSchema;
        }

        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var entities = await _oSchema.GetEntitiesAsync(context.RelationName.Camelize());
            var relations = entities.Select(e => new EntityRelation(new EntityConstraint(e.Type.Name, e.Type, true, true), e))
                .ToArray();

            return relations;
        }
    }
}
