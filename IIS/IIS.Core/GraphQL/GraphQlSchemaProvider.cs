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
        private readonly IOntologyProvider _ontologyProvider;

        public GraphQlSchemaProvider(IServiceProvider serviceProvider, IGraphQlTypeRepository typeRepository, IOntologyProvider ontologyProvider)
        {
            _serviceProvider = serviceProvider;
            _typeRepository = typeRepository;
            _ontologyProvider = ontologyProvider;
        }

        public ISchema GetSchema()
        {
            var builder = SchemaBuilder.New().AddServices(_serviceProvider);
            builder.RegisterTypes(_typeRepository, _ontologyProvider); // TODO: remake dynamic type registration
            builder.AddQueryType<Query>()
                .AddMutationType<Mutation>();
            return builder.Create();
        }
    }

    static class FluentExtensions
    {
        public static ISchemaBuilder RegisterTypes(this ISchemaBuilder schemaBuilder, IGraphQlTypeRepository typeRepository, IOntologyProvider ontologyProvider)
        {
            var creator = new GraphQlTypeCreator(typeRepository, ontologyProvider);
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