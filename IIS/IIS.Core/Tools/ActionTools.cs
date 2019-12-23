using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Seeding;
using IIS.Legacy.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Tools
{
    internal sealed class ActionTools
    {
        private readonly ILogger<ActionTools> _logger;
        private readonly ILegacyOntologyProvider _legacyOntologyProvider;
        private readonly ILegacyMigrator _legacyMigrator;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly OntologyTypeSaver _ontologyTypeSaver;
        private readonly Seeder _seeder;
        private readonly OntologyContext _ontologyContext;

        public ActionTools(
            ILogger<ActionTools> logger,
            ILegacyOntologyProvider legacyOntologyProvider,
            ILegacyMigrator legacyMigrator,
            IOntologyProvider ontologyProvider,
            OntologyTypeSaver ontologyTypeSaver,
            Seeder seeder,
            OntologyContext ontologyContext)
        {
            _logger = logger;
            _legacyOntologyProvider = legacyOntologyProvider;
            _legacyMigrator = legacyMigrator;
            _ontologyProvider = ontologyProvider;
            _ontologyTypeSaver = ontologyTypeSaver;
            _seeder = seeder;
            _ontologyContext = ontologyContext;
        }

        public async Task ClearTypesAsync()
        {
            await _ontologyTypeSaver.ClearTypesAsync();
            _ontologyProvider.Invalidate();
            _logger.LogInformation("Types cleared.");
        }

        public async Task MigrateLegacyTypesAsync()
        {
            var ontology = await _legacyOntologyProvider.GetOntologyAsync();
            _ontologyTypeSaver.SaveTypes(ontology.Types);
            _ontologyProvider.Invalidate();
            _logger.LogInformation("Legacy types migrated.");
        }

        public async Task MigrateLegacyEntitiesAsync()
        {
            await _legacyMigrator.Migrate();
            _logger.LogInformation("Legacy entities migrated.");
        }

        public async Task MigrateLegacyFilesAsync()
        {
            await _legacyMigrator.MigrateFiles();
            _logger.LogInformation("Legacy entities migrated.");
        }

        public async Task FillOdysseusTypesAsync()
        {
            await _seedTypesAsync("odysseus");
        }

        public async Task FillContourTypesAsync()
        {
            await _seedTypesAsync("contour");
        }

        private async Task _seedTypesAsync(string name)
        {
            JsonOntologyProvider provider = new JsonOntologyProvider(Path.Combine(Environment.CurrentDirectory, "data", name, "ontology"));
            Ontology.Ontology ontology = await provider.GetOntologyAsync();
            _ontologyTypeSaver.SaveTypes(ontology.EntityTypes);
            _logger.LogInformation("{name} types filled.", name);
        }

        public async Task SeedContourDataAsync()
        {
            await _seeder.SeedAsync(Path.Combine("contour", "entities"));
            _logger.LogInformation("Contour data seeded.");
        }

        public async Task SeedOdysseusDataAsync()
        {
            await _seeder.SeedAsync(Path.Combine("odysseus", "entities"));
            await SeedOdysseusAnalyticsIndicatorsAsync();
            _logger.LogInformation("Odysseus data seeded.");
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

            _ontologyContext.AnalyticsIndicators.AddRange(b.Indicators);
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
            public List<AnalyticsIndicator> Indicators = new List<AnalyticsIndicator>();

            public AnalyticsIndicator NewIndicator(string title, string query = null)
            {
                var indicator = new AnalyticsIndicator(Guid.NewGuid(), title);

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
