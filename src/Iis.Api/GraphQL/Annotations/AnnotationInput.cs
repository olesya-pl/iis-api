using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Annotations
{
    public class AnnotationInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        [GraphQLNonNullType]
        public string Content { get; set; }
    }
}