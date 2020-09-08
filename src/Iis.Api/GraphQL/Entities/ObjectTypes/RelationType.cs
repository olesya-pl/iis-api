using HotChocolate;
using HotChocolate.Types;
using Iis.Domain;
using System;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    //public class GeoCoordinatesResponse ObjectType<Relation>
    //{
    //    public TypeKind Kind => TypeKind.Object;

    //    public decimal Latitude { get; set; }
    //    public decimal Longitude { get; set; }
    //}
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
            public IEmbeddingRelationTypeModel Type([Parent] Relation parent)
            {
                return parent.Type as IEmbeddingRelationTypeModel;
            }
        }
    }

    public class EmbeddingRelationTypeType : ObjectType<IEmbeddingRelationTypeModel>
    {
        protected override void Configure(IObjectTypeDescriptor<IEmbeddingRelationTypeModel> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);
            descriptor.Include<Resolvers>();
        }

        private class Resolvers
        {
            [GraphQLType(typeof(NonNullType<IdType>))]
            public Guid Id([Parent] IEmbeddingRelationTypeModel parent)
            {
                return parent.Id;
            }

            [GraphQLNonNullType]
            public string Code([Parent] IEmbeddingRelationTypeModel parent)
            {
                return parent.Name;
            }

            [GraphQLNonNullType]
            public string Title([Parent] IEmbeddingRelationTypeModel parent)
            {
                return parent.Title;
            }
        }
    }
}
