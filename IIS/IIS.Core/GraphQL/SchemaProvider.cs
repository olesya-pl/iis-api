using System;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.GraphQL
{
    public class SchemaProvider : ISchemaProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TypeRepository _typeRepository;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IOntologyFieldPopulator _populator;

        public SchemaProvider(IServiceProvider serviceProvider, TypeRepository typeRepository, IOntologyProvider ontologyProvider, IOntologyFieldPopulator populator)
        {
            _serviceProvider = serviceProvider;
            _typeRepository = typeRepository;
            _ontologyProvider = ontologyProvider;
            _populator = populator;
        }

        public ISchema GetSchema()
        {
            var builder = SchemaBuilder.New().AddServices(_serviceProvider);
            RegisterTypes(builder);
            var ontology = TryRegisterOntologyTypes(builder);
            builder.AddQueryType(d =>
            {
                d.Name("QueryType");
                d.Include<EntityTypes.Query>();
                d.Include<Materials.Query>();
                d.Include<Users.Query>();
                if (ontology != null)
                    ConfigureOntologyQuery(d, ontology);
            });
            builder.AddMutationType(d =>
            {
                d.Name("MutationType");
                d.Include<SeederMutation>();
                d.Include<Materials.Mutation>();
                d.Include<DummyMutation>();
                if (ontology != null)
                    ConfigureOntologyMutation(d, ontology);
            });
            return builder.Create();
        }

        public static void RegisterTypes(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>()
                .AddType<EntityTypes.EntityAttributeRelation>();
        }

        public Core.Ontology.Ontology TryRegisterOntologyTypes(ISchemaBuilder schemaBuilder)
        {
            try
            {
                var ontology = _ontologyProvider.GetOntologyAsync().Result; // todo: refactor GetSchema to async
                _typeRepository.InitializeTypes();
                foreach (var type in _typeRepository.AllTypes)
                    schemaBuilder.AddType(type);
                return ontology;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unable to register ontology types");
                Console.Error.WriteLine(ex);
                return null;
            }
        }

        protected void ConfigureOntologyQuery(IObjectTypeDescriptor descriptor, Core.Ontology.Ontology ontology)
        {
            var typesToPopulate = ontology.EntityTypes
//                .Where(t => t.CreateMeta().ExposeOnApi != false)
                .ToList();
            _populator.PopulateFields(descriptor, typesToPopulate, Operation.Read);
            if (typesToPopulate.Count == 0) return;
            ConfigureAllEntitiesQuery(descriptor);
        }

        protected void ConfigureAllEntitiesQuery(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("allEntities")
                .Type(new CollectionType("AllEntities", _typeRepository.GetType<EntityInterface>()))
                .Argument("pagination", d => d.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", d => d.Type<InputObjectType<AllEntitiesFilterInput>>())
                .Resolver(ctx => ctx.Service<IOntologyQueryResolver>().GetAllEntities(ctx));
        }

        protected void ConfigureOntologyMutation(IObjectTypeDescriptor descriptor, Core.Ontology.Ontology ontology)
        {
            var typesToPopulate = ontology.EntityTypes;
//                .Where(t => t.CreateMeta().ExposeOnApi != false);
            typesToPopulate = typesToPopulate.Where(t => !t.IsAbstract);
            _populator.PopulateFields(descriptor, typesToPopulate,
                Operation.Create, Operation.Update, Operation.Delete);
        }
    }
}
