using System;
using System.Collections.Generic;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Seeding;
using IIS.Core.Ontology.Seeding.Odysseus;
using IIS.Legacy.EntityFramework;
using IIS.Core.Analytics.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IIS.Core
{
    public class ConsoleUtilities
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Action> _actions = new Dictionary<string, Action>();

        private void FillActions()
        {
            _actions.Add("clear-types", ClearTypes);
            _actions.Add("migrate-legacy-types", MigrateLegacyTypes);
            _actions.Add("migrate-legacy-entities", MigrateLegacyEntities);
            _actions.Add("migrate-legacy-files", MigrateLegacyFiles);
            _actions.Add("fill-odysseus-types", FillOdysseusTypes);
            _actions.Add("seed-contour-data", SeedContourData);
            _actions.Add("seed-odysseus-data", SeedOdysseusData);
            _actions.Add("apply-ef-migrations", ApplyEfMigrations);
            _actions.Add("help", Help);
        }

        public ConsoleUtilities(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            _configuration = _serviceProvider.GetService<IConfiguration>();
            FillActions();
        }

        /// <summary>
        /// Processes commandline args for applying migrations or seeding instead of starting Kestrel.
        /// </summary>
        /// <returns>true if webserver should start</returns>
        public bool ProcessArguments()
        {
            var action = _configuration["iis-actions"];
            var runServer = _configuration.GetValue<bool>("iis-run-server");
            if (action == null)
                return true;
            var actions = action.Split(",");
            Console.WriteLine($"Received {actions.Length} actions: {string.Join(", ", actions)}");

            foreach (var actionName in actions)
                DoAction(actionName);

            Console.WriteLine($"--iis-run-server is {runServer}");
            return runServer;
        }

        public void ClearTypes()
        {
            _serviceProvider.GetService<OntologyTypeSaver>().ClearTypes();
            _serviceProvider.GetService<IOntologyProvider>().Invalidate();
            Console.WriteLine("Types cleared");
        }

        public void MigrateLegacyTypes()
        {
            var ontology = _serviceProvider.GetService<ILegacyOntologyProvider>().GetOntologyAsync().Result;
            _serviceProvider.GetService<OntologyTypeSaver>().SaveTypes(ontology.Types);
            _serviceProvider.GetService<IOntologyProvider>().Invalidate();
            Console.WriteLine("Legacy types migrated");
        }

        public void MigrateLegacyEntities()
        {
            _serviceProvider.GetService<ILegacyMigrator>().Migrate().Wait();
            Console.WriteLine("Legacy entities migrated");
        }

        public void MigrateLegacyFiles()
        {
            _serviceProvider.GetService<ILegacyMigrator>().MigrateFiles().Wait();
            Console.WriteLine("Legacy entities migrated");
        }

        public void FillOdysseusTypes()
        {
            var ontology = _serviceProvider.GetService<TypeSeeder>().GetOntologyAsync().Result;
            _serviceProvider.GetService<OntologyTypeSaver>().SaveTypes(ontology.Types);
            _serviceProvider.GetService<IOntologyProvider>().Invalidate();
            Console.WriteLine("Odysseys types filled");
        }

        public void SeedContourData()
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetService<Seeder>().Seed("contour").Wait();
                Console.WriteLine("Contour data seeded");
            }
        }

        public void SeedOdysseusData()
        {
            _serviceProvider.GetService<Seeder>().Seed(Path.Combine("odysseus", "entities")).Wait();
            _seedOdysseusAnalyticsIndicators();
            Console.WriteLine("Odysseus data seeded");
        }

        private void _seedOdysseusAnalyticsIndicators()
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
            var ctx = _serviceProvider.GetRequiredService<OntologyContext>();
            ctx.AnalyticsIndicators.AddRange(b.Indicators);
            ctx.SaveChanges();
            Console.WriteLine("Done analytics indicators");
        }

        public void ApplyEfMigrations()
        {
            _serviceProvider
                .GetRequiredService<OntologyContext>()
                .Database
                .Migrate();
            Console.WriteLine("Migration has been applied.");
        }

        public void Help()
        {
            Console.WriteLine("Usage: dotnet IIS.Core.dll --iis-actions action1,action2 [--iis-run-server true]");
            Console.WriteLine("Available actions:");
            foreach (var actionName in _actions.Keys)
                Console.WriteLine($"\t{actionName}");
        }

        public void DoAction(string actionName)
        {
            if (!_actions.TryGetValue(actionName, out var action))
                Console.WriteLine($"Unrecognized action: {actionName}");
            else
            {
                action();
            }
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
