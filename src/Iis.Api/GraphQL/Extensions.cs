using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using Iis.Interfaces.Ontology.Schema;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HotChocolate;
using HotChocolate.Types.Descriptors;
using Iis.Api.GraphQL;
using Iis.Api.Authentication.OntologyJwtBearerAuthentication;

namespace IIS.Core.GraphQL
{
    public static class Extensions
    {
        public static IRequestExecutorBuilder ConfigureSchema(this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder.ConfigureSchema((serviceProvider, schemaBuilder) =>
            {
                var schemaProvider = serviceProvider.GetRequiredService<ISchemaProvider>();

                schemaProvider.ConfigureSchema(schemaBuilder);
                schemaBuilder.ModifyOptions(_ => _.StrictValidation = false);
                schemaBuilder.AddConvention<INamingConventions>(new CompatibilityNamingConvention());
            });
        }

        public static IObjectTypeDescriptor PopulateFields(this IOntologyFieldPopulator populator,
            IObjectTypeDescriptor descriptor,
            IEnumerable<INodeTypeLinked> entityTypes, params Operation[] operations)
        {
            foreach (var type in entityTypes)
                foreach (var operation in operations)
                    populator.AddFields(descriptor, type, operation);
            return descriptor;
        }

        public static IOutputType WrapOutputType(this IOutputType type, INodeTypeLinked relationType)
        {
            if (relationType.IsComputed)
                return type;
            if (relationType.IsRequired)
                return new NonNullType(type);
            if (relationType.IsMultiple)
                return new NonNullType(new ListType(new NonNullType(type)));
            return type;
        }

        public static IInputType WrapInputType(this IInputType type, INodeTypeLinked relationType)
        {
            if (relationType.IsRequired) return new NonNullType(type);
            if (relationType.IsMultiple) return new ListType(new NonNullType(type));
            return type;
        }

        public static IObjectFieldDescriptor ResolverNotImplemented(this IObjectFieldDescriptor d)
        {
            return d.Resolver(_ => throw new NotImplementedException());
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default;
            dictionary.TryGetValue(key, out value);
            return value;
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

        public static TokenPayload GetToken(this IResolverContext context)
        {
            if (context is null
                || !context.ContextData.TryGetValue(TokenPayload.TokenPropertyName, out var tokenPayload)) return null;

            return tokenPayload as TokenPayload;
        }
    }
}