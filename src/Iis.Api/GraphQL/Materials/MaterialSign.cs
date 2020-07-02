using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialSign
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
    }
}
