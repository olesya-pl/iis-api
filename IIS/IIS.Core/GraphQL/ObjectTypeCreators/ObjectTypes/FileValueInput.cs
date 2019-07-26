using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes
{
    // This class represents Ontology.ScalarType.File input type
    public class FileValueInput
    {
        [GraphQLType(typeof(IdType))] public Guid FileId { get; set; }
        [GraphQLNonNullType] public string Title { get; set; }
        public string Type { get; set; }
    }
}