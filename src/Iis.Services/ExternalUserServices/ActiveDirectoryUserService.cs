using Iis.Api.Configuration;
using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text.RegularExpressions;

namespace Iis.Services.ExternalUserServices
{
    public class ActiveDirectoryUserService : IExternalUserService
    {
        private const string UserFilter = "(objectClass=user)";
        private const int OneResult = 1;
        private const string GroupNameRegex = @"=(?'groupName'\S*)\,";
        private const string GroupNameMatch = "groupName";

        ExternalUserServiceConfiguration _configuration;

        public ActiveDirectoryUserService(ExternalUserServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UserSource GetUserSource() => UserSource.ActiveDirectory;

        public bool ValidateCredentials(string username, string password)
        {
            using var context = ConfigurePrincipalContext(_configuration);
            return context.ValidateCredentials(username, password);
        }

        public IEnumerable<ExternalUser> GetUsers()
        {
            using var connection = ConfigureLdapConnection(_configuration);
            var response = connection.Search(
                _configuration.Domain,
                LdapConnection.ScopeSub,
                UserFilter,
                UserAttributes.All,
                false);

            while (response.HasMore())
            {
                if (!TryReadExternalUser(response, out var externalUser))
                    break;

                yield return externalUser;
            }
        }

        public ExternalUser GetUser(string username)
        {
            using var connection = ConfigureLdapConnection(_configuration);
            var cons = new LdapSearchConstraints { MaxResults = OneResult };
            string filter = $"(&{UserFilter}({UserAttributes.UserName}={username}))";
            var response = connection.Search(
                _configuration.Domain,
                LdapConnection.ScopeSub,
                UserFilter,
                UserAttributes.All,
                false,
                cons);

            TryReadExternalUser(response, out var externalUser);

            return externalUser;
        }

        private LdapConnection ConfigureLdapConnection(ExternalUserServiceConfiguration configuration)
        {
            var connection = new LdapConnection();
            connection.Connect(configuration.Server, configuration.Port);
            connection.Bind(configuration.Username, configuration.Password);
            return connection;
        }

        private PrincipalContext ConfigurePrincipalContext(ExternalUserServiceConfiguration configuration)
        {
            return new PrincipalContext(ContextType.Domain, configuration.Server, configuration.Username, configuration.Password);
        }

        private bool TryReadExternalUser(ILdapSearchResults results, out ExternalUser externalUser)
        {
            if (!results.HasMore())
            {
                externalUser = null;
                return false;
            }

            try
            {
                LdapEntry nextEntry = results.Next();
                var attributes = nextEntry.GetAttributeSet();
                string username = attributes[UserAttributes.UserName].StringValue;
                string[] memberOfArray = GetValueArray(attributes, UserAttributes.MemberOf);

                externalUser = new ExternalUser
                {
                    UserName = username,
                    FirstName = GetValue(attributes, UserAttributes.GivenName),
                    SecondName = GetValue(attributes, UserAttributes.MiddleName),
                    LastName = GetValue(attributes, UserAttributes.Cn),
                    Roles = GetExternalRoles(memberOfArray)
                };

                return true;
            }
            catch (LdapReferralException)
            {
                externalUser = null;
                return false;
            }
        }

        private string GetValue(LdapAttributeSet attributes, string attribute) => attributes.GetValueOrDefault(attribute)?.StringValue;

        private string[] GetValueArray(LdapAttributeSet attributes, string attribute) => attributes.GetValueOrDefault(attribute)?.StringValueArray;

        private List<ExternalRole> GetExternalRoles(string[] memberOf) => memberOf is null
            ? new List<ExternalRole>()
            : memberOf.Select(_ => ExtractGroupName(_))
                .Select(ExternalRole.CreateFrom)
                .ToList();

        private string ExtractGroupName(string memberOfItem)
        {
            var match = Regex.Match(memberOfItem, GroupNameRegex);
            if (!match.Success)
                throw new ArgumentException("Invalid group name format", nameof(memberOfItem));

            return match.Groups[GroupNameMatch].Value;
        }
    }
}