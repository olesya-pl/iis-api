using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL
{
    public static class Extensions
    {
        public static IObjectTypeDescriptor PopulateFields(this IOntologyFieldPopulator populator,
            IObjectTypeDescriptor descriptor,
            IEnumerable<EntityType> entityTypes, params Operation[] operations)
        {
            foreach (var type in entityTypes)
            foreach (var operation in operations)
                populator.AddFields(descriptor, type, operation);
            return descriptor;
        }

        public static IOutputType WrapOutputType(this IOutputType type, IEmbeddingRelationTypeModel relationType)
        {
            if (relationType.IsComputed())
                return type;
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

        public static IInputType WrapInputType(this IInputType type, IEmbeddingRelationTypeModel relationType)
        {
            switch (relationType.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                    return type;
                case EmbeddingOptions.Required:
                    return new NonNullType(type);
                case EmbeddingOptions.Multiple:
                    return new ListType(new NonNullType(type)); // Input arrays are optional, opposed to output type
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetFieldName(this IEmbeddingRelationTypeModel relationType)
        {
            return relationType.Name;
        }

        public static EntityOperation[] GetOperations(this IEmbeddingRelationTypeModel relationType)
        {
            if (relationType.IsAttributeType) throw new ArgumentException("Can not check attribute relations for EntityOperations");
            return ((EntityRelationMeta) relationType.Meta)?.AcceptsEntityOperations // check relation meta
                   ?? ((EntityMeta) relationType.TargetType.Meta)?.AcceptsEmbeddedOperations // check target meta
                   ?? relationType.TargetType.AllParents.Reverse().Select(t => t.Meta as EntityMeta) // check target parents meta
                       .FirstOrDefault()?.AcceptsEmbeddedOperations;
        }

        public static bool AcceptsOperation(this IEmbeddingRelationTypeModel relationType, EntityOperation operation)
        {
            var ops = relationType.GetOperations();
            return ops?.Contains(operation) == true;
        }

        public static IObjectFieldDescriptor ResolverNotImplemented(this IObjectFieldDescriptor d)
        {
            return d.Resolver(_ => throw new NotImplementedException());
        }

        public static IObjectFieldDescriptor FieldNotImplemented(this IObjectTypeDescriptor d, string name)
        {
            return d.Field(name).Type<NotImplementedType>().ResolverNotImplemented();
        }

        public static IEnumerable<INodeTypeModel> GetInheritors(this INodeTypeModel type, IEnumerable<INodeTypeModel> ontology)
        {
            return ontology.Where(t =>
                t.RelatedTypes.OfType<InheritanceRelationType>().Any(r => r.ParentType.Name == type.Name));
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue GetOrDefault<TValue>(this IResolverContext context, string key)
        {
            return (TValue) context.ContextData.GetOrDefault(key);
        }

        public static IQueryable<T> GetPage<T>(this IQueryable<T> query, PaginationInput pagination)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (pagination is null)
            {
                throw new ArgumentNullException(nameof(pagination));
            }

            return query.Skip(pagination.Offset()).Take(pagination.PageSize);
        }
    }
}
