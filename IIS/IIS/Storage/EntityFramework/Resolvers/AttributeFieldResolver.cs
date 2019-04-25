using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;

namespace IIS.Storage.EntityFramework.Resolvers
{
    public class AttributeFieldResolver : IFieldResolver
    {
        private readonly bool _isArray;

        public AttributeFieldResolver(bool isArray)
        {
            _isArray = isArray;
        }

        public object Resolve(ResolveFieldContext context)
        {
            var source = (OEntity)context.Source;
            var code = context.FieldName;
            var attrValues = source.AttributeValues.Where(e => e.Attribute.Code == code && e.DeletedAt == null);

            return _isArray ? (object)attrValues.Select(a => new { a.Id, a.Value }).ToArray()
                    : attrValues.SingleOrDefault()?.Value;
        }
    }
}
