using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IIS.Core.Tools
{
    internal sealed class RunUpTools
    {
        private readonly ILogger<RunUpTools> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public RunUpTools(
            ILogger<RunUpTools> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
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
            ActionTools tools = scope.ServiceProvider.GetRequiredService<ActionTools>();

            switch (actionName)
            {
                case "apply-ef-migrations":
                    tools.ApplyEfMigrations();
                    break;
                case "reset-passwords":
                    await tools.ResetPasswordsAsync();
                    break;
                case "update-ontology":
                    tools.UpdateOntology();
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
                "reset-passwords",
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
