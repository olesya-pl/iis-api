using System;
using System.Collections.Generic;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Seeding;
using IIS.Core.Ontology.Seeding.Odysseus;
using IIS.Legacy.EntityFramework;
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
            _serviceProvider.GetService<Seeder>().Seed("contour").Wait();
            Console.WriteLine("Contour data seeded");
        }

        public void SeedOdysseusData()
        {
            _serviceProvider.GetService<Seeder>().Seed("odysseus").Wait();
            Console.WriteLine("Odysseus data seeded");
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
    }
}
