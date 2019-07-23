using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL;
using IIS.Core.GraphQL.Mutations;

namespace IIS.Core.GraphQL
{
    public class GraphQlSchemaProvider : IGraphQLSchemaProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphQlTypeProvider _typeProvider;

        public GraphQlSchemaProvider(IServiceProvider serviceProvider, IGraphQlTypeProvider typeProvider)
        {
            _serviceProvider = serviceProvider;
            _typeProvider = typeProvider;
        }

        public ISchema GetSchema() =>
            SchemaBuilder.New()
                .AddServices(_serviceProvider)
                .RegisterScalars(_typeProvider)
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>() 
                .AddType<EntityTypes.EntityAttributeRelation>()
                .Create();

    }

    static class FluentExtensions
    {
        public static ISchemaBuilder RegisterScalars(this ISchemaBuilder schemaBuilder, IGraphQlTypeProvider typeProvider)
        {
            foreach (var scalarType in typeProvider.Scalars.Values)
                schemaBuilder.AddType(scalarType);
            return schemaBuilder;
        }
    }
    
    public class Query : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor) => descriptor
            .Include<EntityTypes.Query>()
            .Include<Entities.EntityQuery>()
            ;
    }

    public class Mutation : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor) => descriptor
            .Include<DemoMutation>()
            .Include<EntityMutation>()
            ;
    }
}