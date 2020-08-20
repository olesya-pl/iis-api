using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Annotations
{
    public abstract class BaseAnnotation
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
    }
}