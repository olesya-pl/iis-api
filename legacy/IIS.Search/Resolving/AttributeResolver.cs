using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IIS.Search.Resolving
{
    public class AttributeResolver : IRelationResolver
    {
        public async Task<object> ResolveAsync(ResolveContext context)
        {
            var attrName = context.RelationName;
            var src = (JObject)context.Source;
            //var src = hit["_source"];
            var value = src[attrName]?.Value<object>();

            return value;
            //var attributeRelation = ((Entity)relation.Target).GetSingleRelation(attrName);
            //var attribute = (Attribute)attributeRelation.Target;
            //return attribute.Value;
        }
    }
}
