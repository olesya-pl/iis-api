using HotChocolate;
using HotChocolate.Types;
using Iis.Domain;
using System;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class RelationType : ObjectType<Relation>
    {
        protected override void Configure(IObjectTypeDescriptor<Relation> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);
            descriptor.Include<Resolvers>();
        }

        private class Resolvers
        {
            [GraphQLType(typeof(NonNullType<IdType>))]
            public Guid Id([Parent] Relation parent)
            {
                return parent.Id;
            }

//            public DateTime StartsAt([Parent] Relation parent) => throw new NotImplementedException();
//
//            public DateTime EndsAt([Parent] Relation parent) => throw new NotImplementedException();

            public DateTime CreatedAt([Parent] Relation parent)
            {
                return parent.CreatedAt;
            }

            [GraphQLType(typeof(EmbeddingRelationTypeType))]
            public INodeTypeModel Type([Parent] Relation parent) => parent.Type;
        }
    }

    public class EmbeddingRelationTypeType : ObjectType<INodeTypeModel>
    {
        protected override void Configure(IObjectTypeDescriptor<INodeTypeModel> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);
            descriptor.Include<Resolvers>();
        }

        private class Resolvers
        {
            [GraphQLType(typeof(NonNullType<IdType>))]
            public Guid Id([Parent] INodeTypeModel parent) => parent.Id;

            [GraphQLNonNullType]
            public string Code([Parent] INodeTypeModel parent) => parent.Name;

            [GraphQLNonNullType]
            public string Title([Parent] INodeTypeModel parent) => parent.Title;
        }
    }
}
