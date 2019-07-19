using System;
using System.Collections.Generic;
using HotChocolate.Types;
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
        
        // Remake ths to use ScalarType enum
        public static IObjectFieldDescriptor ScalarType(this IObjectFieldDescriptor d, AttributeType attributeType)
        {
            switch (attributeType.ScalarType)
            {
                case "String": d.Type<StringType>(); break;
                case "Int": d.Type<IntType>(); break;
                default:
                    throw new ArgumentException($"Unsupported scalar type: {attributeType.ScalarType}");
            }

            return d;
        }
        
        // Think about merging it with output ScalarType
        public static IInputFieldDescriptor ScalarType(this IInputFieldDescriptor d, AttributeType attributeType)
        {
            switch (attributeType.ScalarType)
            {
                case "String": d.Type<StringType>(); break;
                case "Int": d.Type<IntType>(); break;
                default:
                    throw new ArgumentException($"Unsupported input scalar type: {attributeType.ScalarType}");
            }

            return d;
        }
        
        public static IObjectFieldDescriptor ResolverNotImplemented(this IObjectFieldDescriptor d) =>
            d.Resolver(_ => throw new NotImplementedException());
    }
}