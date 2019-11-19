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
            _serviceProvider.GetService<Seeder>().Seed("odysseus").Wait();
            _seedOdysseusAnalyticsIndicators();
            Console.WriteLine("Odysseus data seeded");
        }

        private void _seedOdysseusAnalyticsIndicators()
        {
            var b = new AnalyticsIndicatorBuilder();
            var root = b.NewIndicator(title: "Підрозділи СБУ", code: "sbu")
                .AddChild(
                    b.NewIndicator(title: "Орган/підрозділ СБУ", code: "sbu")
                        .AddChild(
                            b.NewIndicator(title: "Кількість працівників", code: "amountOfWorkers")
                                .AddChild(b.NewIndicator(title: "За штатом", code: "byStaff"))
                                .AddChild(b.NewIndicator(title: "Фактична", code: "real"))
                        )
                        .AddChild(
                            b.NewIndicator(title: "Допуск до ДТ", code: "accessToStateSecrets")
                                .AddChild(b.NewIndicator(title: "Всього надано", code: "provided"))
                                .AddChild(b.NewIndicator(title: "Форма 1", code: "levelForm1"))
                                .AddChild(b.NewIndicator(title: "Форма 2", code: "levelForm2"))
                                .AddChild(b.NewIndicator(title: "Форма 3", code: "levelForm3"))
                        )
                        .AddChild(
                            b.NewIndicator(title: "Перевірки", code: "vettingProcedures")
                                .AddChild(b.NewIndicator(title: "Всі проведені", code: "all"))
                                .AddChild(b.NewIndicator(title: "Комплексні", code: "complex"))
                                .AddChild(b.NewIndicator(title: "Тематичні", code: "thematic"))
                                .AddChild(b.NewIndicator(title: "Контрольні", code: "control"))
                        )
                        .AddChild(
                            b.NewIndicator(title: "Заходи впливу", code: "sanctions")
                                .AddChild(
                                    b.NewIndicator(title: "Всі СРСД", code: "organization")
                                        .AddChild(b.NewIndicator(title: "Інформування", code: "organization"))
                                        .AddChild(b.NewIndicator(title: "Зупинення спецдозволу", code: "specialPermitSuspensionSanction"))
                                        .AddChild(b.NewIndicator(title: "Скасування спецдозволу", code: "specialPermitCancellationSanction"))
                                )
                                .AddChild(
                                    b.NewIndicator(title: "Секретоносій", code: "person")
                                        .AddChild(b.NewIndicator(title: "Дисциплінарна відповідальність", code: "disciplinaryPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Протокол", code: "reportPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Скасування допуска", code: "cancellationPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Службове розслідування", code: "investigationPersonSanction"))
                                        .AddChild(b.NewIndicator(title: "Кримінальне провадження", code: "criminalPersonSanction"))
                                )
                        )
                )
                .AddChild(
                    b.NewIndicator(title: "НДДКР", code: "srddw")
                        .AddChild(b.NewIndicator(title: "Всі НДДКР", code: "all"))
                        .AddChild(b.NewIndicator(title: "Т", code: "secret"))
                        .AddChild(b.NewIndicator(title: "ЦТ", code: "absoluteSecret"))
                        .AddChild(b.NewIndicator(title: "ОВ", code: "veryImportant"))
                )
                .AddChild(
                    b.NewIndicator(title: "СРСД", code: "organizations")
                        .AddChild(b.NewIndicator(title: "Всі СРСД", code: "all"))
                        .AddChild(b.NewIndicator(title: "Військові", code: "military"))
                        .AddChild(b.NewIndicator(title: "Цивільні", code: "civil"))
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

            public AnalyticsIndicator NewIndicator(string title, string code)
            {
                var indicator = new AnalyticsIndicator(Guid.NewGuid(), title, code);
                Indicators.Add(indicator);
                return indicator;
            }
        }
    }
}
