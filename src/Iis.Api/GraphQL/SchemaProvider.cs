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
using Iis.Api.Ontology;
using Iis.Api.GraphQL;
using Iis.Api.GraphQL.CreateMenu;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.Extensions.Logging;

namespace IIS.Core.GraphQL
{
    public class SchemaProvider : ISchemaProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TypeRepository _typeRepository;
        private readonly IOntologySchema _ontologySchema;
        private readonly IOntologyFieldPopulator _populator;
        private readonly IConfiguration _configuration;
        private ISchema _hotChocolateSchema;
        private readonly ILogger<SchemaProvider> _logger;

        public SchemaProvider(IServiceProvider serviceProvider, 
            TypeRepository typeRepository, 
            IOntologySchema ontologySchema, 
            IOntologyFieldPopulator populator, 
            IConfiguration configuration,
            ILogger<SchemaProvider> logger)
        {
            _serviceProvider = serviceProvider;
            _typeRepository = typeRepository;
            _ontologySchema = ontologySchema;
            _populator = populator;
            _configuration = configuration;
            _logger = logger;
        }

        public ISchema GetSchema()
        {
            return _hotChocolateSchema ?? (_hotChocolateSchema = LoadSchema());
        }

        public void RecreateSchema()
        {
            _hotChocolateSchema = LoadSchema();
        }

        private ISchema LoadSchema()
        {
            _logger.LogInformation("SchemaProvider. LoadSchema. Starting schema load");
            var builder = SchemaBuilder.New().AddServices(_serviceProvider);
            RegisterTypes(builder);
            TryRegisterOntologyTypes(builder);
            builder.AddQueryType(d =>
            {
                d.Name("QueryType");
                d.Include<ObjectSignQuery>();
                d.Include<EntityTypes.Query>();
                d.Include<Materials.Query>();
                d.Include<Roles.Query>();
                d.Include<OntologyFilterableQuery>();
                d.Include<Users.Query>();
                d.Include<AnalyticsQuery.Query>();
                d.Include<AnalyticsIndicator.Query>();
                d.Include<ExportQuery>();
                d.Include<ML.Query>();
                d.Include<ElasticConfig.Query>();
                d.Include<ChangeHistory.Query>();
                d.Include<Themes.Query>();
                d.Include<Autocomplete.Query>();
                d.Include<Iis.Api.GraphQL.Aliases.Query>();
                d.Include<Annotations.Query>();
                d.Include<AssociatedEventsQuery>();
                d.Include<IncomingEntitiesQuery>();
                d.Include<RelationsCountQuery>();
                d.Include<CreateMenuItemsQuery>();

                if (_configuration.GetValue("reportsAvailable", true))
                {
                    d.Include<Reports.Query>();
                }
                if (_ontologySchema != null)
                    ConfigureOntologyQuery(d, _ontologySchema);
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
                d.Include<Iis.Api.GraphQL.Aliases.Mutation>();

                if (_configuration.GetValue("reportsAvailable", true))
                {
                    d.Include<Reports.Mutation>();
                }
                if (_ontologySchema != null)
                    ConfigureOntologyMutation(d, _ontologySchema);
            });
            try
            {
                var res = builder.Create();
                _logger.LogInformation("SchemaProvider. LoadSchema. Ending building schema");
                return res;
            }
            catch (Exception e)
            {
                _logger.LogInformation("SchemaProvider. LoadSchema. Exception {e}", e);
                if (e.InnerException != null)
                {
                    _logger.LogInformation("SchemaProvider. LoadSchema. InnerException {e}", e.InnerException);
                }
                _logger.LogInformation("SchemaProvider. LoadSchema. Attempting retry");
                return builder.Create();
            }
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

        protected void ConfigureOntologyQuery(IObjectTypeDescriptor descriptor, IOntologySchema schema)
        {
            var typesToPopulate = schema.GetEntityTypes().ToList();
            _logger.LogInformation($"SchemaProvider. ConfigureOntologyQuery. Fetched {typesToPopulate.Count} items. These are {string.Join(',', typesToPopulate.Select(p => p.Name))}");
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

        protected void ConfigureOntologyMutation(IObjectTypeDescriptor descriptor, IOntologySchema schema)
        {
            var typesToPopulate = schema.GetEntityTypes();
            typesToPopulate = typesToPopulate.Where(t => !t.IsAbstract).ToList();            
            _logger.LogInformation($"SchemaProvider. ConfigureOntologyMutation. Fetched {typesToPopulate.Count()} items. These are {string.Join(',', typesToPopulate.Select(p => p.Name))}");
            _populator.PopulateFields(descriptor, typesToPopulate,
                Operation.Create, Operation.Update, Operation.Delete);
        }
    }
}
