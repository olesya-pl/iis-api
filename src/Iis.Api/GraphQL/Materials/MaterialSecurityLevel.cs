using HotChocolate;
using HotChocolate.Types;
using System;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialSecurityLevel
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int UniqueIndex { get; set; }
    }
}
