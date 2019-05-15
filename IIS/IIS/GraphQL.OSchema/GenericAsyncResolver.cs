using System;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.OSchema;

namespace IIS.GraphQL.OSchema
{
    public class GenericAsyncResolver : AsyncFieldResolver<object>
    {
        private static Func<ResolveFieldContext, Task<object>> GetResolveMethod(IRelationResolver resolver)
        {
            return (ResolveFieldContext context) =>
            {
                var newContext = new ResolveContext
                {
                    Parameters = context.Arguments,
                    RelationName = context.FieldName,
                    Source = context.Source
                };

                return resolver.ResolveAsync(newContext);
            };
        }

        public GenericAsyncResolver(IRelationResolver resolver) : base(GetResolveMethod(resolver)) { }
    }
}
