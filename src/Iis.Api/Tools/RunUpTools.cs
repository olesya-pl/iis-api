using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Iis.Application.Ontology;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IIS.Core.Tools
{
    internal sealed class RunUpTools
    {
        private readonly ILogger<RunUpTools> _logger;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public RunUpTools(
            ILogger<RunUpTools> logger,
            IMediator mediator,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _mediator = mediator;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Processes commandline args for applying migrations or seeding instead of starting Kestrel.
        /// </summary>
        /// <returns>true if web server should start</returns>
        public async Task<bool> ProcessArgumentsAsync()
        {
            string action = _configuration["iis-actions"];
            if (action == null)
            {
                return true;
            }

            string[] actions = action.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (actions.Length == 0)
            {
                return true;
            }

            _logger.LogInformation("Received {count} actions: {actions}", actions.Length, string.Join(", ", actions));
            foreach (string actionName in actions)
            {
                try
                {
                    using (_logger.BeginScope("Action: {0}", actionName))
                    {
                        await DoActionAsync(actionName);
                    }
                }
                catch (DataException e)
                {
                    _logger.LogCritical(e, "Action {action} failed.", actionName);
                    return false;
                }
            }

            return _configuration.GetValue<bool>("iis-run-server");
        }

        public async Task DoActionAsync(string actionName)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            if (actionName == "save-contour-ontology")
            {
                await _mediator.Send(new SaveOntologyCommand("contour"));
                return;
            }
            
            ActionTools tools = scope.ServiceProvider.GetRequiredService<ActionTools>();

            switch (actionName)
            {
                case "clear-types":
                    await tools.ClearTypesAsync();
                    break;
                case "migrate-legacy-types":
                    await tools.MigrateLegacyTypesAsync();
                    break;
                case "migrate-legacy-entities":
                    await tools.MigrateLegacyEntitiesAsync();
                    break;
                case "migrate-legacy-files":
                    await tools.MigrateLegacyFilesAsync();
                    break;
                case "fill-odysseus-types":
                    await tools.FillOdysseusTypesAsync();
                    break;
                case "fill-contour-types":
                    await tools.FillContourTypesAsync();
                    break;
                case "fill-developer-types":
                    await tools.FillDeveloperTypesAsync();
                    break;
                case "seed-contour-data":
                    await tools.SeedContourDataAsync();
                    break;
                case "seed-odysseus-data":
                    await tools.SeedOdysseusDataAsync();
                    break;
                case "seed-developer-data":
                    await tools.SeedDeveloperDataAsync();
                    break;
                case "apply-ef-migrations":
                    tools.ApplyEfMigrations();
                    break;
                case "dump-contour-ontology":
                    await tools.DumpContourOntologyAsync();
                    break;
                case "dump-odysseus-ontology":
                    await tools.DumpOdysseusOntologyAsync();
                    break;
                case "help":
                    Help();
                    break;
                default:
                    _logger.LogInformation("Unrecognized action: {action}", actionName);
                    break;
            }
        }

        public void Help()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Usage: dotnet IIS.Core.dll --iis-actions action1,action2 [--iis-run-server true]");
            sb.AppendLine("Available actions:");

            List<string> actions = new List<string>
            {
                "clear-types",
                "migrate-legacy-types",
                "migrate-legacy-entities",
                "migrate-legacy-files",
                "fill-odysseus-types",
                "fill-contour-types",
                "seed-contour-data",
                "seed-odysseus-data",
                "apply-ef-migrations",
                "dump-contour-ontology",
                "dump-odysseus-ontology",
                "help"
            };
            foreach (string actionName in actions)
            {
                sb.AppendLine($"\t{actionName}");
            }

            _logger.LogInformation(sb.ToString());
        }
    }
}
