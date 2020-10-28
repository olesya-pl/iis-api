using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using Iis.DataModel;
using Iis.DataModel.Analytics;
using Iis.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Iis.Api;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using IIS.Domain;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.Interfaces.Ontology.Schema;
using System.Configuration;
using Iis.OntologySchema.Saver;
using Iis.DbLayer.OntologySchema;

namespace IIS.Core.Tools
{
    internal sealed class ActionTools
    {
        private readonly ILogger<ActionTools> _logger;
        private readonly IConfiguration _configuration;
        private readonly OntologyContext _ontologyContext;
        private readonly RunTimeSettings _runtimeSettings;
        OntologySchemaService _ontologySchemaService;

        public ActionTools(
            ILogger<ActionTools> logger,
            IConfiguration configuration,
            OntologyContext ontologyContext,
            RunTimeSettings runTimeSettings,
            OntologySchemaService ontologySchemaService)
        {
            _logger = logger;
            _configuration = configuration;
            _ontologyContext = ontologyContext;
            _runtimeSettings = runTimeSettings;
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
