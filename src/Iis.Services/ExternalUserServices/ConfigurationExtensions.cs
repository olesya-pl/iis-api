using Iis.Api.Configuration;
using Novell.Directory.Ldap;
using System.DirectoryServices.AccountManagement;

namespace Iis.Services.ExternalUserServices
{
    internal static class ConfigurationExtensions
    {
        public static LdapConnection ConfigureLdapConnection(this ExternalUserServiceConfiguration configuration)
        {
            var connection = new LdapConnection();
            connection.Connect(configuration.Server, configuration.Port);
            connection.Bind(configuration.Username, configuration.Password);
            return connection;
        }

        public static PrincipalContext ConfigurePrincipalContext(this ExternalUserServiceConfiguration configuration)
        {
            return new PrincipalContext(ContextType.Domain, configuration.Server, configuration.Username, configuration.Password);
        }
    }
}