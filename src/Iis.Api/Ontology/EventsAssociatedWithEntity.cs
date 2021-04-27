using System;
using HotChocolate;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities.ObjectTypes;

namespace Iis.Api.Ontology
{
    public class EventAssociatedWithEntity
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [GraphQLType(typeof(PredictableDateType))]
        public DateTime? StartsAt { get; set; }
        [GraphQLType(typeof(PredictableDateType))]
        public DateTime? EndsAt { get; set; }
        public EventState State { get; set; }
        public EventImportance Importance { get; set; }
    }

    public class EventState
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class EventImportance
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}