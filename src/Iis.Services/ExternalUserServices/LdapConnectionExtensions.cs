using Iis.Api.Configuration;
using Novell.Directory.Ldap;

namespace Iis.Services.ExternalUserServices
{
    internal static class LdapConnectionExtensions
    {
        public static ILdapSearchResults MakeRequest(
            this LdapConnection connection,
            ExternalUserServiceConfiguration configuration,
            string filter,
            string[] includedFields)
        {
            return connection.Search(
                configuration.Domain,
                LdapConnection.ScopeSub,
                filter,
                includedFields,
                false);
        }

        public static ILdapSearchResults MakeRequest(
            this LdapConnection connection,
            ExternalUserServiceConfiguration configuration,
            string filter,
            string[] includedFields,
            LdapSearchConstraints cons)
        {
            return connection.Search(
                configuration.Domain,
                LdapConnection.ScopeSub,
                filter,
                includedFields,
                false,
                cons);
        }
    }
}