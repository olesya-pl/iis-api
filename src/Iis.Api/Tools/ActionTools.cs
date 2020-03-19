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
using Iis.Api.Ontology.Migration;
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
        private readonly IOntologyProvider _ontologyProvider;
        private readonly OntologyTypeSaver _ontologyTypeSaver;
        private readonly Seeder _seeder;
        private readonly OntologyContext _ontologyContext;
        private readonly RunTimeSettings _runtimeSettings;
        private readonly MigrationService _migrationService;
        OntologySchemaService _ontologySchemaService;

        public ActionTools(
            ILogger<ActionTools> logger,
            IConfiguration configuration,
            IOntologyProvider ontologyProvider,
            OntologyTypeSaver ontologyTypeSaver,
            Seeder seeder,
            OntologyContext ontologyContext,
            RunTimeSettings runTimeSettings,
            MigrationService migrationService,
            OntologySchemaService ontologySchemaService)
        {
            _logger = logger;
            _configuration = configuration;
            _ontologyProvider = ontologyProvider;
            _ontologyTypeSaver = ontologyTypeSaver;
            _seeder = seeder;
            _ontologyContext = ontologyContext;
            _runtimeSettings = runTimeSettings;
            _migrationService = migrationService;
            _ontologySchemaService = ontologySchemaService;
        }

        public async Task ClearTypesAsync()
        {
            await _ontologyTypeSaver.ClearTypesAsync();
            _ontologyProvider.Invalidate();
            _logger.LogInformation("Types cleared.");
        }

        public async Task FillOdysseusTypesAsync()
        {
            await SeedTypesAsync("odysseus");
        }

        public async Task FillContourTypesAsync()
        {
            await SeedTypesAsync("contour");
        }
        
        public async Task FillDeveloperTypesAsync()
        {
            await SeedTypesAsync("develop");
        }

        private async Task SeedTypesAsync(string name)
        {
            JsonOntologyProvider provider = new JsonOntologyProvider(Path.Combine(Environment.CurrentDirectory, "data", name, "ontology"));
            OntologyModel ontology = await provider.GetOntologyAsync();
            _ontologyTypeSaver.SaveTypes(ontology.EntityTypes);
            _logger.LogInformation("{name} types filled.", name);
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

        public async Task DumpOdysseusOntologyAsync()
            {
            await _dumpOntology("odysseus");
        }

        public async Task DumpContourOntologyAsync()
        {
            await _dumpOntology("contour");
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

        public async Task MigrateOntologyAsync()
        {
            _runtimeSettings.PutSavedToElastic = false;
            ApplyEfMigrations();
            var rulesFileName = Path.Combine(Environment.CurrentDirectory, "data", "contour", "migrations", "001", "rules.json");
            var json = File.ReadAllText(rulesFileName);
            var rules = JsonConvert.DeserializeObject<MigrationRules>(json);
            var ontologyMigration = new OntologyMigrationsEntity
            {
                Id = Guid.NewGuid(),
                OrderNumber = rules.OrderNumber, 
                StartTime = DateTime.Now
            };
            _migrationService.SetRules(rules);
            
            _migrationService.MakeSnapshotOld();
            
            await ClearTypesAsync();
            await FillContourTypesAsync();
            
            var migrationResult = await _migrationService.MigrateAsync();
            ontologyMigration.StructureBefore = migrationResult.StructureBefore;
            ontologyMigration.StructureAfter = migrationResult.StructureAfter;
            ontologyMigration.MigrationRules = migrationResult.MigrationRules;
            ontologyMigration.Log = migrationResult.Log;
            ontologyMigration.IsSuccess = migrationResult.IsSuccess;
            _ontologyContext.OntologyMigrations.Add(ontologyMigration);
            _ontologyContext.SaveChanges();
            if (!migrationResult.IsSuccess)
            {
                _logger.LogError("Error during migration: {error}", migrationResult.Log);
                return;
            }
            await _seeder.SeedAsync(Path.Combine("contour", "entities"), rules.AllowedEntities);
            _logger.LogInformation("Migration is Success!!!");
            _runtimeSettings.PutSavedToElastic = true;
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
        }

        private async Task _dumpOntology(string name)
        {
            var ontology = await _ontologyProvider.GetOntologyAsync();
            var basePath = Path.Combine(Environment.CurrentDirectory, "data", name, "ontology");
            var serializer = new Serializer();

            serializer.serialize(basePath, ontology);
            Console.WriteLine($"Dumped ontology for {name} into {basePath}");
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
