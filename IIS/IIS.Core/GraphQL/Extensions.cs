using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;
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

        public static HotChocolate.Types.ScalarType GetScalarType(this IGraphQlTypeProvider typeProvider, AttributeType attributeType)
            => typeProvider.Scalars[attributeType.ScalarType.ToLower()];
        
        // Remake ths to use ScalarType enum
        /// <summary>
        /// Sets field type with known scalar type considering RelationType's embedding options (NonNull or/and List)
        /// </summary>
        public static IObjectFieldDescriptor ScalarType(this IObjectFieldDescriptor d, EmbeddingRelationType relationType, IGraphQlTypeProvider typeProvider)
            => d.Type(typeProvider.GetScalarType(relationType.AttributeType).WrapOutputType(relationType));
        
        public static IInputFieldDescriptor ScalarType(this IInputFieldDescriptor d, AttributeType attributeType, IGraphQlTypeProvider typeProvider)
            => d.Type(typeProvider.GetScalarType(attributeType)); // no wrapping currently

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

        public static IObjectFieldDescriptor ResolverNotImplemented(this IObjectFieldDescriptor d) =>
            d.Resolver(_ => throw new NotImplementedException());
    }
}