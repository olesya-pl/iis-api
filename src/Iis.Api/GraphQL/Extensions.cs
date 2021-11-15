using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL
{
    public static class Extensions
    {
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
            if(context is null) return null;

            return context.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;
        }

    }
}
