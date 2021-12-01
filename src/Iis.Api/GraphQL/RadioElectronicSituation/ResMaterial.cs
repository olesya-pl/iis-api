using System;
using HotChocolate;
using HotChocolate.Types;
using Iis.Api.GraphQL.Common;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResMaterial
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public IdTitle Caller { get; set; }
        public IdTitle Receiver { get; set; }
        public ResMaterialFile File { get; set; }
        public ResMaterialMetadata Metadata { get; set; }
    }
}
