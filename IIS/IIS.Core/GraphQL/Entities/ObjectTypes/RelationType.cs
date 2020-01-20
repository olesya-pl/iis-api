using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Ontology;
using Iis.Domain;

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
            public EmbeddingRelationType Type([Parent] Relation parent)
            {
                return parent.Type as EmbeddingRelationType;
            }
        }
    }

    public class EmbeddingRelationTypeType : ObjectType<EmbeddingRelationType>
    {
        protected override void Configure(IObjectTypeDescriptor<EmbeddingRelationType> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);
            descriptor.Include<Resolvers>();
        }

        private class Resolvers
        {
            [GraphQLType(typeof(NonNullType<IdType>))]
            public Guid Id([Parent] EmbeddingRelationType parent)
            {
                return parent.Id;
            }

            [GraphQLNonNullType]
            public string Code([Parent] EmbeddingRelationType parent)
            {
                return parent.Name;
            }

            [GraphQLNonNullType]
            public string Title([Parent] EmbeddingRelationType parent)
            {
                return parent.Title;
            }
        }
    }
}
