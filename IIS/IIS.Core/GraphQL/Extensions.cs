using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL
{
    public static class Extensions
    {
        // Query should depend on OntologyRepository. Delete this method after.
        public static IEnumerable<Type> GetTypes(this IOntologyProvider ontologyProvider)
        {
            var task = ontologyProvider.GetTypesAsync();
            task.Wait();
            return task.Result;
        }

        public static IObjectTypeDescriptor PopulateFields(this IObjectTypeDescriptor descriptor, IOntologyProvider ontologyProvider, ITypeFieldPopulator populator)
        {
            var types = ontologyProvider.GetTypes().OfType<EntityType>();
            foreach (var type in types)
                populator.AddFields(descriptor, type);
            return descriptor;
        }

        public static HotChocolate.Types.ScalarType GetScalarType(this IGraphQlTypeRepository typeRepository, AttributeType attributeType)
            => typeRepository.Scalars[attributeType.ScalarTypeEnum];
        
        /// <summary>
        /// Sets field type with known scalar type considering RelationType's embedding options (NonNull or/and List)
        /// </summary>
        public static IOutputType GetOutputAttributeType(this IGraphQlTypeRepository typeRepository, AttributeType attributeType)
        {
            IOutputType type;
            if (attributeType.ScalarTypeEnum == Core.Ontology.ScalarType.File)
                type = typeRepository.GetType<ObjectType<Attachment>>();
            else
                type = typeRepository.GetScalarType(attributeType);
            return type;
        }

        public static IOutputType WrapOutputType(this IOutputType type, EmbeddingRelationType relationType)
        {
            switch (relationType.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                    return type;
                case EmbeddingOptions.Required:
                    return new NonNullType(type);
                case EmbeddingOptions.Multiple:
                    return new NonNullType(new ListType(new NonNullType(type)));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static IInputType WrapInputType(this IInputType type, EmbeddingRelationType relationType)
        {
            switch (relationType.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                    return type;
                case EmbeddingOptions.Required:
                    return new NonNullType(type);
                case EmbeddingOptions.Multiple:
                    return new NonNullType(new ListType(new NonNullType(type)));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetFieldName(this EmbeddingRelationType relationType) => relationType.Name;

        [Obsolete("Remove this abomination asap. Was created for testing.")]
        public static bool AcceptsOperation(this EmbeddingRelationType relationType, EntityOperation operation) =>
            (relationType.CreateMeta() as RelationTypeMeta)?.AcceptsEntityOperations?.Contains(operation) == true;

        public static IObjectFieldDescriptor ResolverNotImplemented(this IObjectFieldDescriptor d) =>
            d.Resolver(_ => throw new NotImplementedException());
        
        public static IObjectFieldDescriptor FieldNotImplemented(this IObjectTypeDescriptor d, string name) =>
            d.Field(name).Type<NotImplementedType>().ResolverNotImplemented();
    }
}