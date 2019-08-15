using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL
{
    public static class Extensions
    {
        public static IObjectTypeDescriptor PopulateFields(this ITypeFieldPopulator populator, IObjectTypeDescriptor descriptor,
            IEnumerable<EntityType> entityTypes, params Operation[] operations)
        {
            foreach (var type in entityTypes)
                foreach (var operation in operations)
                    populator.AddFields(descriptor, type, operation);
            return descriptor;
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

        public static bool AcceptsOperation(this EmbeddingRelationType relationType, EntityOperation operation) =>
            ((EntityRelationMeta)relationType.CreateMeta()).AcceptsEntityOperations?.Contains(operation) == true;

        public static IObjectFieldDescriptor ResolverNotImplemented(this IObjectFieldDescriptor d) =>
            d.Resolver(_ => throw new NotImplementedException());
        
        public static IObjectFieldDescriptor FieldNotImplemented(this IObjectTypeDescriptor d, string name) =>
            d.Field(name).Type<NotImplementedType>().ResolverNotImplemented();

        public static IEnumerable<Type> GetInheritors(this Type type, IEnumerable<Type> ontology) =>
            ontology.Where(t => t.RelatedTypes.OfType<InheritanceRelationType>().Any(r => r.ParentType.Name == type.Name));

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default;
            dictionary.TryGetValue(key, out value);
            return value;
        }
        
        public static TValue GetOrDefault<TValue>(this IResolverContext context, string key) =>
            (TValue) context.ContextData.GetOrDefault(key);
    }
}