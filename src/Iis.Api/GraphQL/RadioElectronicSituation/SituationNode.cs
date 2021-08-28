using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class SituationNode
    {
        public Geometry Geometry { get; set; }

        public Attributes Attributes { get; set; }
    }

    public class Geometry
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class Attributes
    {
        [GraphQLType(typeof(IdType))]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TypeTitle { get; set; }
        public string TypeName { get; set; }
        public string Sidc { get; set; }
        public DateTime LastConfirmedAt { get; set; }
    }
}