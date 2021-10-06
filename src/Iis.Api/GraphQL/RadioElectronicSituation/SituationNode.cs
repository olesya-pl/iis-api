using System;
using System.Collections.Generic;
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
        public IReadOnlyCollection<ObjectNode> ObjectOfStudyCollection { get; set; }
        public IReadOnlyCollection<SignNode> SignCollection { get; set; }
        public IReadOnlyCollection<MaterialNode> MaterialCollection { get; set; }
    }

    public class ObjectNode : BaseObjectNode
    {
        public string Title { get; set; }
        public string Sidc { get; set; }
        public string NickName { get; set; }
        [GraphQLType(typeof(UuidType))]
        public Guid SignId { get; }
    }

    public class SignNode : BaseObjectNode
    {
        public string Value { get; set; }
    }

    public class MaterialNode
    {
        [GraphQLType(typeof(IdType))]
        public Guid Id { get; set; }
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
        [GraphQLType(typeof(UuidType))]
        public Guid? MaterialId { get; set; }
        public string MaterialRegistrationDate { get; set; }
    }
}