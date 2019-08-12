using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL
{
    public class GraphQlSchemaProvider : IGraphQLSchemaProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphQlTypeRepository _typeRepository;
        private readonly IOntologyRepository _ontologyRepository;

        public GraphQlSchemaProvider(IServiceProvider serviceProvider, IGraphQlTypeRepository typeRepository, IOntologyRepository ontologyRepository)
        {
            _serviceProvider = serviceProvider;
            _typeRepository = typeRepository;
            _ontologyRepository = ontologyRepository;
        }

        public ISchema GetSchema()
        {
            var builder = SchemaBuilder.New().AddServices(_serviceProvider);
            builder.RegisterTypes(_typeRepository, _ontologyRepository); // TODO: remake dynamic type registration
            builder.AddQueryType<Query>()
                .AddMutationType<Mutation>();
            return builder.Create();
        }
    }

    static class FluentExtensions
    {
        public static ISchemaBuilder RegisterTypes(this ISchemaBuilder schemaBuilder, IGraphQlTypeRepository typeRepository, IOntologyRepository ontologyRepository)
        {
            var creator = new GraphQlTypeCreator(typeRepository, ontologyRepository);
            creator.Create();
            foreach (var type in typeRepository.AllTypes)
                schemaBuilder.AddType(type);
            schemaBuilder // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>()
                .AddType<EntityTypes.EntityAttributeRelation>();
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