using Iis.DataModel;
using Iis.DbLayer.ModifyDataScripts;
using Iis.EventMaterialAutoAssignment;
using Iis.FlightRadar.DataModel;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Iis.Api
{
    internal static class HostExtensions
    {
        private static int OntologyMigrateTimeout = 10;

        public static async Task SeedExternalUsersAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            try
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.ImportUsersFromExternalSourceAsync();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<LogHeaderMiddleware>>();
                logger.LogError($"Cannot import external users: {ex.Message}");
            }
        }

        public static async Task MigrateDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            await scope.ServiceProvider.MigrateDatabaseAsync<OntologyContext>(TimeSpan.FromMinutes(OntologyMigrateTimeout));
            await scope.ServiceProvider.MigrateDatabaseSafetyAsync<FlightsContext>();
            await scope.ServiceProvider.MigrateDatabaseSafetyAsync<AssignmentConfigContext>();
        }

        public static bool RunModifyDataScripts(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var modifyDataRunner = scope.ServiceProvider.GetService<ModifyDataRunner>();
            return modifyDataRunner.Run();
        }

        private static async Task MigrateDatabaseSafetyAsync<TDbContext>(this IServiceProvider serviceProvider, TimeSpan? commandTimeout = null)
            where TDbContext : DbContext
        {
            try
            {
                await serviceProvider.MigrateDatabaseAsync<TDbContext>(commandTimeout);
            }
            catch
            {
            }
        }

        private static async Task MigrateDatabaseAsync<TDbContext>(this IServiceProvider serviceProvider, TimeSpan? commandTimeout = null)
            where TDbContext : DbContext
        {
            var context = serviceProvider.GetService<TDbContext>();
            var defaultCommandTimeout = context.Database.GetCommandTimeout();
            if (commandTimeout is not null)
            {
                context.Database.SetCommandTimeout(commandTimeout.Value);
            }

            await context.Database.MigrateAsync();

            if (commandTimeout is not null)
            {
                context.Database.SetCommandTimeout(defaultCommandTimeout);
            }
        }
    }
}