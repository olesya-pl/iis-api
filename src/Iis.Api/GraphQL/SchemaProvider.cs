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
using Iis.Api.Ontology;
using Iis.Api.GraphQL.Graph;
using Iis.Api.GraphQL.CreateMenu;
using Iis.Api.GraphQL.RadioElectronicSituation;
using Microsoft.Extensions.Configuration;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.Extensions.Logging;
using Iis.Api.GraphQL.NodeMaterialRelation;
using Iis.Api.Authorization;

namespace IIS.Core.GraphQL
{
    public class SchemaProvider : ISchemaProvider
    {
        private const string ReportsAvailableConfiguration = "reportsAvailable";

        private readonly TypeRepository _typeRepository;
        private readonly IOntologySchema _ontologySchema;
        private readonly IOntologyFieldPopulator _populator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SchemaProvider> _logger;

        public SchemaProvider(
            TypeRepository typeRepository,
            IOntologySchema ontologySchema,
            IOntologyFieldPopulator populator,
            IConfiguration configuration,
            ILogger<SchemaProvider> logger)
        {
            _typeRepository = typeRepository;
            _ontologySchema = ontologySchema;
            _populator = populator;
            _configuration = configuration;
            _logger = logger;
        }

        public void ConfigureSchema(ISchemaBuilder schemaBuilder)
        {
            _logger.LogInformation("SchemaProvider. LoadSchema. Starting schema load");

            RegisterTypes(schemaBuilder);
            TryRegisterOntologyTypes(schemaBuilder);
            ConfigureQueryType(schemaBuilder);
            ConfigureMutationType(schemaBuilder);

            _logger.LogInformation("SchemaProvider. LoadSchema. Ending building schema");
        }

        protected void ConfigureOntologyQuery(IObjectTypeDescriptor descriptor, IOntologySchema schema)
        {
            var typesToPopulate = schema.GetEntityTypes().ToList();
            _logger.LogInformation($"SchemaProvider. ConfigureOntologyQuery. Fetched {typesToPopulate.Count} items. These are {string.Join(',', typesToPopulate.Select(_ => _.Name))}");
            _populator.PopulateFields(descriptor, typesToPopulate, Operation.Read).Authorize(Policies.HasGrant);
            if (typesToPopulate.Count == 0) return;
            ConfigureAllEntitiesQueries(descriptor);
        }

        protected void ConfigureAllEntitiesQueries(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("allEntities")
                .Type(new CollectionType("AllEntities", _typeRepository.GetType<EntityInterface>()))
                .Argument("pagination", _ => _.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", _ => _.Type<InputObjectType<AllEntitiesFilterInput>>())
                .Resolver(_ => _.Service<IOntologyQueryResolver>().GetAllEntities(_))
                .Authorize(Policies.HasGrant);
        }

        protected void ConfigureOntologyMutation(IObjectTypeDescriptor descriptor, IOntologySchema schema)
        {
            var typesToPopulate = schema.GetEntityTypes();
            typesToPopulate = typesToPopulate.Where(_ => !_.IsAbstract).ToList();
            _logger.LogInformation($"SchemaProvider. ConfigureOntologyMutation. Fetched {typesToPopulate.Count()} items. These are {string.Join(',', typesToPopulate.Select(_ => _.Name))}");
            _populator.PopulateFields(
                descriptor,
                typesToPopulate,
                Operation.Create,
                Operation.Update,
                Operation.Delete)
                .Authorize(Policies.HasGrant);
        }

        private static void RegisterTypes(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder // TODO: Find a better way to register interface implementation types
                .AddType<EntityTypes.EntityAttributePrimitive>()
                .AddType<EntityTypes.EntityAttributeRelation>();
        }

        private void TryRegisterOntologyTypes(ISchemaBuilder schemaBuilder)
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

        private void ConfigureQueryType(ISchemaBuilder builder)
        {
            builder.AddQueryType(_ =>
            {
                _.Name("QueryType");
                _.Include<ObjectSignQuery>().Authorize(Policies.HasGrant);
                _.Include<EntityTypes.Query>().Authorize(Policies.HasGrant);
                _.Include<Materials.Query>().Authorize(Policies.HasGrant);
                _.Include<Roles.Query>().Authorize(Policies.HasGrant);
                _.Include<OntologyFilterableQuery>().Authorize(Policies.HasGrant);
                _.Include<Users.Query>().Authorize(Policies.HasGrant);
                _.Include<AnalyticsQuery.Query>().Authorize(Policies.HasGrant);
                _.Include<AnalyticsIndicator.Query>().Authorize(Policies.HasGrant);
                _.Include<ExportQuery>().Authorize(Policies.HasGrant);
                _.Include<ML.Query>().Authorize(Policies.HasGrant);
                _.Include<ElasticConfig.Query>().Authorize(Policies.HasGrant);
                _.Include<ChangeHistory.Query>().Authorize(Policies.HasGrant);
                _.Include<Themes.Query>().Authorize(Policies.HasGrant);
                _.Include<Autocomplete.Query>().Authorize(Policies.HasGrant);
                _.Include<Iis.Api.GraphQL.Aliases.Query>().Authorize(Policies.HasGrant);
                _.Include<Annotations.Query>().Authorize(Policies.HasGrant);
                _.Include<AssociatedEventsQuery>().Authorize(Policies.HasGrant);
                _.Include<IncomingEntitiesQuery>().Authorize(Policies.HasGrant);
                _.Include<RelationsCountQuery>().Authorize(Policies.HasGrant);
                _.Include<CreateMenuItemsQuery>().Authorize(Policies.HasGrant);
                _.Include<GraphQuery>().Authorize(Policies.HasGrant);
                _.Include<ResQuery>().Authorize(Policies.HasGrant);
                _.Include<NodeMaterialRelationQuery>().Authorize(Policies.HasGrant);

                if (_configuration.GetValue(ReportsAvailableConfiguration, true))
                {
                    _.Include<Reports.Query>().Authorize(Policies.HasGrant);
                }

                if (_ontologySchema != null)
                {
                    ConfigureOntologyQuery(_, _ontologySchema);
                }
            });
        }

        private void ConfigureMutationType(ISchemaBuilder builder)
        {
            builder.AddMutationType(_ =>
            {
                _.Name("MutationType");
                _.Include<Materials.Mutation>().Authorize(Policies.HasGrant);
                _.Include<Users.Mutation>().Authorize(Policies.HasGrant);
                _.Include<Roles.Mutation>().Authorize(Policies.HasGrant);
                _.Include<Users.LoginResolver>();
                _.Include<AnalyticsQuery.Mutation>().Authorize(Policies.HasGrant);
                _.Include<ML.Mutation>().Authorize(Policies.HasGrant);
                _.Include<NodeMaterialRelation.Mutation>().Authorize(Policies.HasGrant);
                _.Include<ElasticConfig.Mutation>().Authorize(Policies.HasGrant);
                _.Include<Themes.Mutation>().Authorize(Policies.HasGrant);
                _.Include<Annotations.Mutation>().Authorize(Policies.HasGrant);
                _.Include<Iis.Api.GraphQL.Aliases.Mutation>().Authorize(Policies.HasGrant);

                if (_configuration.GetValue(ReportsAvailableConfiguration, true))
                {
                    _.Include<Reports.Mutation>().Authorize(Policies.HasGrant);
                }

                if (_ontologySchema != null)
                {
                    ConfigureOntologyMutation(_, _ontologySchema);
                }
            });
        }
    }
}