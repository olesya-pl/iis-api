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
        public ObjectOfStudy ObjectOfStudy { get; set; }
        public ObjectSign Sign { get; set; }
        public Material Material { get; set; }
    }

    public class ObjectOfStudy : Object
    {
        public string Title { get; set; }
        public string Sidc { get; set; }
        public string LastConfirmAt { get; set; }
    }

    public class ObjectSign : Object
    {
        public string Value { get; set; }
    }

    public class Material
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string CreatedDate { get; set; }
        public string RegistrationDate { get; set; }
    }

    public abstract class Object
    {
        [GraphQLType(typeof(IdType))]
        public Guid Id { get; set; }
        public string TypeTitle { get; set; }
        public string TypeName { get; set; }
    }
}