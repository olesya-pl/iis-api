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

        public GraphQlSchemaProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISchema GetSchema() =>
            SchemaBuilder.New()
                .AddServices(_serviceProvider)
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>() 
                .AddType<EntityTypes.EntityAttributeRelation>()
                .Create();
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