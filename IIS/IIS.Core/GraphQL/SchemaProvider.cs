using System;
using HotChocolate;
using IIS.Core.GraphQL.Entities;

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
            RegisterTypes(builder);
            var ontologyRegistered = TryRegisterOntologyTypes(builder, _typeRepository);
            builder.AddQueryType(d =>
            {
                d.Name("QueryType");
                d.Include<EntityTypes.Query>();
                d.Include<Materials.Query>();
                if (ontologyRegistered)
                    d.Include<Entities.QueryEndpoint>();
            });
            builder.AddMutationType(d =>
            {
                d.Name("MutationType");
                d.Include<DummyMutation>();
                d.Include<Materials.Mutation>();
                if (ontologyRegistered)
                    d.Include<Entities.MutationEndpoint>();
            });
            return builder.Create();
        }

        public static void RegisterTypes(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>()
                .AddType<EntityTypes.EntityAttributeRelation>();
        }

        public static bool TryRegisterOntologyTypes(ISchemaBuilder schemaBuilder, TypeRepository repository)
        {
            try
            {
                repository.InitializeTypes();
                foreach (var type in repository.AllTypes)
                    schemaBuilder.AddType(type);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unable to register ontology types");
                Console.Error.WriteLine(ex);
                return false;
            }
        }
    }
}
