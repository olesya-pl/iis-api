using Iis.Services.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services
{
    public class ConnectionStringService : IConnectionStringService
    {
        private IConfiguration _configuration;
        public ConnectionStringService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetConnectionString(string name, string prefix)
        {
            if (_configuration == null) throw new ArgumentNullException(nameof(_configuration));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(_configuration.GetConnectionString(name));

            string host = _configuration[prefix + "HOST"];
            if (host != null)
            {
                connectionStringBuilder.Host = host;
            }

            string database = _configuration[prefix + "NAME"];
            if (database != null)
            {
                connectionStringBuilder.Database = database;
            }

            string username = _configuration[prefix + "USERNAME"];
            if (username != null)
            {
                connectionStringBuilder.Username = username;
            }

            string password = _configuration[prefix + "PASSWORD"];
            if (string.IsNullOrEmpty(connectionStringBuilder.Password) && password != null)
            {
                connectionStringBuilder.Password = password;
            }

            return connectionStringBuilder.ConnectionString;
        }
        public string GetIisApiConnectionString() =>
            GetConnectionString("db", "DB_");

        public string GetFlightRadarConnectionString() =>
            GetConnectionString("db-flightradar", "DB_");
    }
}
