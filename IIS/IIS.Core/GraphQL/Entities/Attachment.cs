using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities
{
    // This class represents Ontology.ScalarType.File output type
    public class Attachment
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid FileId { get; set; }
        [GraphQLNonNullType] public string Title { get; set; }
        public string Type { get; set; }
        [GraphQLNonNullType] public string Url { get; set; }
    }
}