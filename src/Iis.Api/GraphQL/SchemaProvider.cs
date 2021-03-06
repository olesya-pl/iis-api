using System;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.GraphQL.Export;
using Iis.Domain;
using Microsoft.Extensions.Configuration;
using IIS.Domain;
using Iis.Api.Ontology;

namespace IIS.Core.GraphQL
{
    public class SchemaProvider : ISchemaProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TypeRepository _typeRepository;
        private readonly IOntologyModel _ontology;
        private readonly IOntologyFieldPopulator _populator;
        private readonly IConfiguration _configuration;
        private ISchema _schema;

        public SchemaProvider(IServiceProvider serviceProvider, TypeRepository typeRepository, IOntologyModel ontology, IOntologyFieldPopulator populator, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _typeRepository = typeRepository;
            _ontology = ontology;
            _populator = populator;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public ISchema GetSchema()
        {
            return _schema ?? (_schema = LoadSchema());
        }

        public void RecreateSchema()
        {
            _schema = LoadSchema();
        }

        private ISchema LoadSchema()
        {
            var builder = SchemaBuilder.New().AddServices(_serviceProvider);
            RegisterTypes(builder);
            TryRegisterOntologyTypes(builder);
            builder.AddQueryType(d =>
            {
                d.Name("QueryType");
                d.Include<Entities.ObjectSignQuery>();
                d.Include<EntityTypes.Query>();
                d.Include<Materials.Query>();
                d.Include<Roles.Query>();
                d.Include<Entities.ObjectOfStudyFilterableQuery>();
                d.Include<Users.Query>();
                d.Include<AnalyticsQuery.Query>();
                d.Include<AnalyticsIndicator.Query>();
                d.Include<ExportQuery>();
                d.Include<ML.Query>();
                d.Include<ElasticConfig.Query>();
                d.Include<ChangeHistory.Query>();
                d.Include<Themes.Query>();
                d.Include<Autocomplete.Query>();
                d.Include<Annotations.Query>();
                d.Include<AssociatedEventsQuery>();
                d.Include<IncomingEntitiesQuery>();

                if (_configuration.GetValue("reportsAvailable", true))
                {
                    d.Include<Reports.Query>();
                }
                if (_ontology != null)
                    ConfigureOntologyQuery(d, _ontology);
            });
            builder.AddMutationType(d =>
            {
                d.Name("MutationType");
                d.Include<Materials.Mutation>();
                d.Include<Users.Mutation>();
                d.Include<Roles.Mutation>();
                d.Include<Users.LoginResolver>();
                d.Include<AnalyticsQuery.Mutation>();
                d.Include<ML.Mutation>();
                d.Include<NodeMaterialRelation.Mutation>();
                d.Include<ElasticConfig.Mutation>();
                d.Include<Themes.Mutation>();
                d.Include<Files.Mutation>();
                d.Include<Annotations.Mutation>();

                if (_configuration.GetValue("reportsAvailable", true))
                {
                    d.Include<Reports.Mutation>();
                }
                if (_ontology != null)
                    ConfigureOntologyMutation(d, _ontology);
            });
            return builder.Create();
        }

        public static void RegisterTypes(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>()
                .AddType<EntityTypes.EntityAttributeRelation>();
        }

        public void TryRegisterOntologyTypes(ISchemaBuilder schemaBuilder)
        {
            try
            {
                _typeRepository.InitializeTypes();
                foreach (var type in _typeRepository.AllTypes)
                {
                    schemaBuilder.AddType(type);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unable to register ontology types");
                Console.Error.WriteLine(ex);
            }
        }

        protected void ConfigureOntologyQuery(IObjectTypeDescriptor descriptor, IOntologyModel ontology)
        {
            var typesToPopulate = ontology.EntityTypes
//                .Where(t => t.CreateMeta().ExposeOnApi != false)
                .ToList();
            _populator.PopulateFields(descriptor, typesToPopulate, Operation.Read);
            if (typesToPopulate.Count == 0) return;
            ConfigureAllEntitiesQueries(descriptor);
        }

        protected void ConfigureAllEntitiesQueries(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("allEntities")
                .Type(new CollectionType("AllEntities", _typeRepository.GetType<EntityInterface>()))
                .Argument("pagination", d => d.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", d => d.Type<InputObjectType<AllEntitiesFilterInput>>())
                .Resolver(ctx => ctx.Service<IOntologyQueryResolver>().GetAllEntities(ctx));
        }

        protected void ConfigureOntologyMutation(IObjectTypeDescriptor descriptor, IOntologyModel ontology)
        {
            var typesToPopulate = ontology.EntityTypes;
            typesToPopulate = typesToPopulate.Where(t => !t.IsAbstract);
            _populator.PopulateFields(descriptor, typesToPopulate,
                Operation.Create, Operation.Update, Operation.Delete);
        }
    }
}
