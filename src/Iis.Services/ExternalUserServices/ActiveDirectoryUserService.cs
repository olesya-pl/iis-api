using Iis.Api.Configuration;
using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using Novell.Directory.Ldap;
using System.Collections.Generic;

namespace Iis.Services.ExternalUserServices
{
    public class ActiveDirectoryUserService : IExternalUserService
    {
        private const string UserFilter = "(objectClass=user)";
        private const int OneResult = 1;

        ExternalUserServiceConfiguration _configuration;

        public ActiveDirectoryUserService(ExternalUserServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UserSource GetUserSource() => UserSource.ActiveDirectory;

        public bool ValidateCredentials(string username, string password)
        {
            using var context = _configuration.ConfigurePrincipalContext();
            return context.ValidateCredentials(username, password);
        }

        public IEnumerable<ExternalUser> GetUsers()
        {
            using var connection = _configuration.ConfigureLdapConnection();
            var response = connection.MakeRequest(_configuration, UserFilter, UserAttributes.All);

            while (response.HasMore())
            {
                if (!response.TryReadOne(out var externalUser))
                    break;

                yield return externalUser;
            }
        }

        public ExternalUser GetUser(string username)
        {
            using var connection = _configuration.ConfigureLdapConnection();
            var cons = new LdapSearchConstraints { MaxResults = OneResult };
            string filter = $"(&{UserFilter} ({UserAttributes.UserName}={username}))";
            var response = connection.MakeRequest(_configuration, filter, UserAttributes.All, cons);

            response.TryReadOne(out var externalUser);

            return externalUser;
        }
    }
}