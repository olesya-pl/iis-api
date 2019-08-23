using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.EntityTypes;

namespace IIS.Core.GraphQL
{
    public class SchemaProvider : ISchemaProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TypeRepository _typeRepository;

        public SchemaProvider(IServiceProvider serviceProvider, TypeRepository typeRepository)
        {
            _serviceProvider = serviceProvider;
            _typeRepository = typeRepository;
        }

        public ISchema GetSchema()
        {
            var builder = SchemaBuilder.New().AddServices(_serviceProvider);
            builder.RegisterTypes(_typeRepository); // TODO: remake dynamic type registration
            builder.AddQueryType<Query>()
                .AddMutationType<Mutation>();
            return builder.Create();
        }
    }

    internal static class FluentExtensions
    {
        public static ISchemaBuilder RegisterTypes(this ISchemaBuilder schemaBuilder, TypeRepository repository)
        {
            repository.InitializeTypes();
            foreach (var type in repository.AllTypes)
                schemaBuilder.AddType(type);
            schemaBuilder // TODO: Find a better way to register interface implementation types
                .AddType<EntityAttributePrimitive>()
                .AddType<EntityAttributeRelation>();
            return schemaBuilder;
        }
    }

    public class Query : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor
                .Include<EntityTypes.Query>()
                .Include<QueryEndpoint>();
        }
    }

    public class Mutation : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor
                .Include<DummyMutation>()
                .Include<MutationEndpoint>();
        }
    }
}
