using HotChocolate;
using HotChocolate.Types;
using System;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResMaterialFile
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Url { get; set; }
    }
}
