using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class CreateMaterialResponse
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
    }
}
