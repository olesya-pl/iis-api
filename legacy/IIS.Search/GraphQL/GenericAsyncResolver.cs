using System;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Search.Resolving;

namespace IIS.Search.GraphQL
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
