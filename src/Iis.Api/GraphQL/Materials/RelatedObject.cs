using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class RelatedObject
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string NodeType { get; set; }
        public string RelationType { get; set; }
        public string RelationCreatingType { get; set; }
    }
}