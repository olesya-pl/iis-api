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
        public ObjectNode ObjectOfStudy { get; set; }
        public SignNode Sign { get; set; }
        public MaterialNode Material { get; set; }
    }

    public class ObjectNode : BaseObjectNode
    {
        public string Title { get; set; }
        public string Sidc { get; set; }
        public string LastConfirmAt { get; set; }
    }

    public class SignNode : BaseObjectNode
    {
        public string Value { get; set; }
    }

    public class MaterialNode
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string CreatedDate { get; set; }
        public string RegistrationDate { get; set; }
    }

    public abstract class BaseObjectNode
    {
        [GraphQLType(typeof(IdType))]
        public Guid Id { get; set; }
        public string TypeTitle { get; set; }
        public string TypeName { get; set; }
    }
}