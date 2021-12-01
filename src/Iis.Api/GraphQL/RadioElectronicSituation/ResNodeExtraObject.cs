using HotChocolate;
using HotChocolate.Types;
using System;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResNodeExtraObject
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public ResNodeExtraObjectBePartOf BePartOf { get; set; }
        public ResNodeExtraObjectAffiliation Affiliation { get; set; }
        public ResNodeExtraObjectAmount Amount { get; set; }
    }
}
