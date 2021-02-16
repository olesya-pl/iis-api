using Iis.DataModel;
using Iis.DataModel.Analytics;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.Saver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IIS.Core.Tools
{
    internal sealed class ActionTools
    {
        private readonly ILogger<ActionTools> _logger;
        private readonly IConfiguration _configuration;
        private readonly OntologyContext _ontologyContext;
        OntologySchemaService _ontologySchemaService;

        public ActionTools(
            ILogger<ActionTools> logger,
            IConfiguration configuration,
            OntologyContext ontologyContext,
            OntologySchemaService ontologySchemaService)
        {
            _logger = logger;
            _configuration = configuration;
            _ontologyContext = ontologyContext;
            _ontologySchemaService = ontologySchemaService;
        }

        public void ApplyEfMigrations()
        {
            _ontologyContext.Database.Migrate();
            _logger.LogInformation("Migration has been applied.");
        }
        
        public async Task ResetPasswordsAsync()
        {
            List<UserEntity> userEntities = await _ontologyContext.Users.ToListAsync();
            foreach (UserEntity userEntity in userEntities)
            {
                userEntity.PasswordHash = _configuration.GetPasswordHashAsBase64String("123");
            }

            await _ontologyContext.SaveChangesAsync();
        }

        public void UpdateOntology()
        {
            var schemaFileName = Path.Combine(Environment.CurrentDirectory, "data", "contour", "migrations", "002", "migration-002.ont");
            var schemaFrom = _ontologySchemaService.GetOntologySchema(new OntologySchemaSource { SourceKind = SchemaSourceKind.File, Data = schemaFileName });
            var connectionString = _configuration.GetConnectionString("db", "DB_");
            var schemaTo = _ontologySchemaService.GetOntologySchema(new OntologySchemaSource { SourceKind = SchemaSourceKind.Database, Data = connectionString });
            var compareResult = schemaFrom.CompareTo(schemaTo);
            var schemaSaver = new OntologySchemaSaver(OntologyContext.GetContext(connectionString), schemaTo);
            schemaSaver.SaveToDatabase(compareResult);
            var ontologyMigration = new OntologyMigrationsEntity
            {
                Id = Guid.NewGuid(),
                OrderNumber = 2,
                StartTime = DateTime.Now
            };
            ontologyMigration.StructureBefore = JsonConvert.SerializeObject(schemaTo.GetRawData(), Formatting.Indented);
            ontologyMigration.StructureAfter = JsonConvert.SerializeObject(schemaTo.GetRawData(), Formatting.Indented);
            ontologyMigration.MigrationRules = string.Empty;
            ontologyMigration.Log = string.Empty;
            ontologyMigration.IsSuccess = true;
            _ontologyContext.OntologyMigrations.Add(ontologyMigration);
            _ontologyContext.SaveChanges();
        }

        class AnalyticsIndicatorBuilder
        {
            public List<AnalyticIndicatorEntity> Indicators = new List<AnalyticIndicatorEntity>();

            public AnalyticIndicatorEntity NewIndicator(string title, string query = null)
            {
                var indicator = new AnalyticIndicatorEntity(Guid.NewGuid(), title);

                if (query != null)
                    indicator.Query = _readQueryFile(query);

                Indicators.Add(indicator);
                return indicator;
            }

            public string _readQueryFile(string query)
            {
                var queryPath = Path.Combine(Environment.CurrentDirectory, "data", "odysseus", "analyticsQuery", $"{query}.json");

                using (var reader = new StreamReader(File.OpenRead(queryPath)))
                {
                    var content = reader.ReadToEnd();
                    JObject.Parse(content);
                    return content;
                }
            }
        }
    }
}
