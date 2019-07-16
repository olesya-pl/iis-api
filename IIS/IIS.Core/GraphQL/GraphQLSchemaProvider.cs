using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL;
using IIS.Core.GraphQL.Mutations;

namespace IIS.Core.GraphQL
{
    public static class GraphQlSchemaProvider
    {
        public static ISchema GetSchema(IServiceProvider s) =>
            SchemaBuilder.New()
                .AddServices(s)
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
            //.Include<Entities.Query>()
            ;
    }

    public class Mutation : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor) => descriptor
            .Include<DemoMutation>();
    }
}