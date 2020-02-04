using System;
using Microsoft.Extensions.Configuration;

namespace IIS.Core
{
    internal static class ConnectionStringExtensions
    {
        public static string GetConnectionString(this IConfiguration configuration, string name, string prefix)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(configuration.GetConnectionString(name));

            string host = configuration[prefix + "HOST"];
            if (host != null)
            {
                connectionStringBuilder.Host = host;
            }

            string database = configuration[prefix + "NAME"];
            if (database != null)
            {
                connectionStringBuilder.Database = database;
            }

            string username = configuration[prefix + "USERNAME"];
            if (username != null)
            {
                connectionStringBuilder.Username = username;
            }

            string password = configuration[prefix + "PASSWORD"];
            if (string.IsNullOrEmpty(connectionStringBuilder.Password) && password != null)
            {
                connectionStringBuilder.Password = password;
            }

            return connectionStringBuilder.ConnectionString;
        }
    }
}
