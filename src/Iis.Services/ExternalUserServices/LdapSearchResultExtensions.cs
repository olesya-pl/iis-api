using Iis.Services.Contracts.ExternalUserServices;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Iis.Services.ExternalUserServices
{
    internal static class LdapSearchResultExtensions
    {
        private const string GroupNameRegex = @"=(?'groupName'\S*)\,";
        private const string GroupNameMatch = "groupName";

        public static bool TryReadOne(this ILdapSearchResults results, out ExternalUser externalUser)
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

                externalUser = new ExternalUser
                {
                    UserName = username,
                    FirstName = attributes.GetValue(UserAttributes.GivenName),
                    SecondName = attributes.GetValue(UserAttributes.MiddleName),
                    LastName = attributes.GetValue(UserAttributes.Cn),
                    Roles = attributes.GetValueArray(UserAttributes.MemberOf).GetExternalRoles()
                };

                return true;
            }
            catch (LdapException)
            {
                externalUser = null;
                return false;
            }
        }

        private static string GetValue(this LdapAttributeSet attributes, string attribute) => attributes.GetValueOrDefault(attribute)?.StringValue;

        private static string[] GetValueArray(this LdapAttributeSet attributes, string attribute) => attributes.GetValueOrDefault(attribute)?.StringValueArray;

        private static List<ExternalRole> GetExternalRoles(this string[] memberOf) => memberOf is null
            ? new List<ExternalRole>()
            : memberOf.Select(_ => _.ExtractGroupName())
                .Select(ExternalRole.CreateFrom)
                .ToList();

        private static string ExtractGroupName(this string memberOfItem)
        {
            var match = Regex.Match(memberOfItem, GroupNameRegex);
            if (!match.Success)
                throw new ArgumentException("Invalid group name format", nameof(memberOfItem));

            return match.Groups[GroupNameMatch].Value;
        }
    }
}