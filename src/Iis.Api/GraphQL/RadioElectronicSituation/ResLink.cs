using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResLink
    {
        public ResLinkExtra Extra { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid From { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid To { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
    }
}
