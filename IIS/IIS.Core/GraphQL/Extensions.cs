using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    }
}