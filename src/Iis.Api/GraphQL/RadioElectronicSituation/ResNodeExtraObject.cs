using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResNodeExtraObject
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string TitlePhoto { get; set; }
        public string LastConfirmedAt { get; set; }
        public ResNodeExtraObjectBePartOf BePartOf { get; set; }
        public ResNodeExtraObjectAffiliation Affiliation { get; set; }
        public ResNodeExtraObjectAmount Amount { get; set; }
        public ResNodeExtraObjectClassifiers Classifiers { get; set; }
        public ResNodeExtraObjectCountry Country { get; set; }
        public ResNodeExtraObjectCountry RelatesToCountry { get; set; }
    }
}
