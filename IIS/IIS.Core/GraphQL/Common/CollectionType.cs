using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Common
{
    public class CollectionType<T> : ObjectType<IEnumerable<T>>
    {
        protected override void Configure(IObjectTypeDescriptor<IEnumerable<T>> descriptor)
        {
            descriptor.Name($"{typeof(T).Name}Collection");
            descriptor.BindFieldsExplicitly().Include<Resolvers>();
        }

        public class Resolvers
        {
            public int GetCount([Parent] IEnumerable<T> parent) => parent.Count();

            [GraphQLNonNullType]
            public IEnumerable<T> GetItems([Parent] IEnumerable<T> parent, [GraphQLNonNullType] PaginationInput pagination)
            {
                return parent.Skip(pagination.Offset()).Take(pagination.PageSize);
            }
        }
    }
}
