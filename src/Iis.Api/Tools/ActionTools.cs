using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.Seeding;
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
        private readonly Seeder _seeder;
        private readonly OntologyContext _ontologyContext;
        private readonly RunTimeSettings _runtimeSettings;
        OntologySchemaService _ontologySchemaService;

        public ActionTools(
            ILogger<ActionTools> logger,
            IConfiguration configuration,
            Seeder seeder,
            OntologyContext ontologyContext,
            RunTimeSettings runTimeSettings,
            OntologySchemaService ontologySchemaService)
        {
            _logger = logger;
            _configuration = configuration;
            _seeder = seeder;
            _ontologyContext = ontologyContext;
            _runtimeSettings = runTimeSettings;
            _ontologySchemaService = ontologySchemaService;
        }

        public async Task SeedContourDataAsync()
        {
            _runtimeSettings.PutSavedToElastic = false;
            await _seeder.SeedAsync(Path.Combine("contour", "entities"));
            _logger.LogInformation("Contour data seeded.");
            _runtimeSettings.PutSavedToElastic = true;
        }

        public async Task SeedDeveloperDataAsync()
        {
            _runtimeSettings.PutSavedToElastic = false;
            await _seeder.SeedAsync(Path.Combine("develop", "entities"));
            _logger.LogInformation("Develop data seeded.");
            _runtimeSettings.PutSavedToElastic = true;
        }
        
        public async Task SeedOdysseusDataAsync()
        {
            _runtimeSettings.PutSavedToElastic = false;
            await _seeder.SeedAsync(Path.Combine("odysseus", "entities"));
            await SeedOdysseusAnalyticsIndicatorsAsync();
            _logger.LogInformation("Odysseus data seeded.");
            _runtimeSettings.PutSavedToElastic = true;
        }

        private async Task SeedOdysseusAnalyticsIndicatorsAsync()
        {
            var b = new AnalyticsIndicatorBuilder();
            var root = b.NewIndicator(title: "Підрозділи СБУ", query: "sbuOrgs")
                .AddChild(
                    b.NewIndicator(title: "Орган/підрозділ СБУ")
                        .AddChild(
                            b.NewIndicator(title: "Кількість працівників")
                                .AddChild(b.NewIndicator(title: "За штатом", query: "employeesByStaff"))
                                .AddChild(b.NewIndicator(title: "Фактична", query: "actualEmployees"))
                        )
                        .AddChild(
                            b.NewIndicator(title: "Допуск до ДТ")
                                .AddChild(b.NewIndicator(title: "Всього надано", query: "providedAccess"))
                                .AddChild(b.NewIndicator(title: "Форма 1", query: "accessForm1"))
                                .AddChild(b.NewIndicator(title: "Форма 2", query: "accessForm2"))
                                .AddChild(b.NewIndicator(title: "Форма 3", query: "accessForm3"))
                        )
                        .AddChild(
                            b.NewIndicator(title: "Перевірки")
                                .AddChild(b.NewIndicator(title: "Всі проведені", query: "allVettingProcedures"))
                                .AddChild(b.NewIndicator(title: "Комплексні", query: "complexVettingProcedures"))
                                .AddChild(b.NewIndicator(title: "Тематичні", query: "thematicVettingProcedures"))
                                .AddChild(b.NewIndicator(title: "Контрольні", query: "controlVettingProcedures"))
                        )
                        .AddChild(
                            b.NewIndicator(title: "Заходи впливу")
                                .AddChild(
                                    b.NewIndicator(title: "Всі СРСД")
                                        .AddChild(b.NewIndicator(title: "Інформування", query: "informingSanction"))
                                        .AddChild(b.NewIndicator(title: "Зупинення спецдозволу", query: "specialPermitSuspensionSanction"))
                                        .AddChild(b.NewIndicator(title: "Скасування спецдозволу", query: "specialPermitCancellationSanction"))
                                )
                                .AddChild(
                                    b.NewIndicator(title: "Секретоносій")
                                        .AddChild(b.NewIndicator(title: "Дисциплінарна відповідальність", query: "disciplinaryPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Протокол", query: "reportPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Скасування допуска", query: "cancellationPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Службове розслідування", query: "investigationPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Кримінальне провадження", query: "criminalPersonSanction"))
                                )
                        )
                )
                .AddChild(
                    b.NewIndicator(title: "НДДКР")
                        .AddChild(b.NewIndicator(title: "Всі НДДКР", query: "allSrddw"))
                        .AddChild(b.NewIndicator(title: "Т", query: "secretSrddw"))
                        .AddChild(b.NewIndicator(title: "ЦТ", query: "absoluteSecretSrddw"))
                        .AddChild(b.NewIndicator(title: "ОВ", query: "veryImportantSrddw"))
                )
                .AddChild(
                    b.NewIndicator(title: "СРСД")
                        .AddChild(b.NewIndicator(title: "Всі СРСД", query: "allOrgs"))
                        .AddChild(b.NewIndicator(title: "Військові", query: "militaryOrgs"))
                        .AddChild(b.NewIndicator(title: "Цивільні", query: "civilOrgs"))
                )
            ;

            _ontologyContext.AnalyticIndicators.AddRange(b.Indicators);
            await _ontologyContext.SaveChangesAsync();

            _logger.LogInformation("Done analytics indicators.");
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
            var schemaSaver = new OntologySchemaSaver(OntologyContext.GetContext(connectionString));
            schemaSaver.SaveToDatabase(compareResult, schemaTo);
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
