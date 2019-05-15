using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.OSchema.Resolving
{
    public class RelationInfoResolver : IRelationResolver
    {
        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var entity = (EntityValue)context.Source;
            return entity.RelationInfo.Any() ? entity.RelationInfo : null;
        }
    }

    public class RelationInfoAttributeResolver : IRelationResolver
    {
        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var attrName = context.RelationName;
            var relationInfo = (IEnumerable<AttributeRelation>)context.Source;
            var target = relationInfo.SingleOrDefault(_ => _.Name == attrName)?.Target;
            return target;
        }
    }
}
