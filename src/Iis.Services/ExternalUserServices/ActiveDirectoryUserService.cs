using Iis.Api.Configuration;
using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Iis.Services.ExternalUserServices
{
    public class ActiveDirectoryUserService : IExternalUserService
    {
        ExternalUserServiceConfiguration _configuration;
        
        public ActiveDirectoryUserService(ExternalUserServiceConfiguration configuration)
        {
            _configuration = configuration;
        }
        public UserSource GetUserSource() => UserSource.ActiveDirectory;
        private LdapConnection GetLdapConnection()
        {
            var connection = new LdapConnection();
            connection.Connect(_configuration.Server, _configuration.Port);
            connection.Bind(_configuration.Username, _configuration.Password);
            return connection;
        }
        public bool ValidateCredentials(string username, string password)
        {
            using (var ctx = GetPrincipalContext())
            {
                return ctx.ValidateCredentials(username, password);
            }

        }
        private PrincipalContext GetPrincipalContext() =>
            new PrincipalContext(ContextType.Domain, _configuration.Server, _configuration.Username, _configuration.Password);

        private ILdapSearchResults MakeRequest(
            LdapConnection connection, 
            string filter, 
            string[] includedFields)
        {
            return connection.Search(
                _configuration.Domain, 
                LdapConnection.ScopeSub, 
                filter, 
                includedFields, 
                false);
        }

        public List<ExternalUser> GetUsers()
        {
            using var ldapConn = GetLdapConnection();
            
            var includedFields = new string[] { "samAccountName", "givenname", "middlename", "sn", "memberOf" };
            var response = MakeRequest(ldapConn, "(objectClass=user)", includedFields);
            var result = new List<ExternalUser>();

            while (response.HasMore())
            {
                try
                {
                    LdapEntry nextEntry = response.Next();
                    var attributes = nextEntry.GetAttributeSet();
                    var username = attributes["samAccountName"].StringValue;
                    
                    var externalUser = new ExternalUser
                    {
                        UserName = username,
                        FirstName = attributes.GetValueOrDefault("givenname")?.StringValue,
                        SecondName = attributes.GetValueOrDefault("middlename")?.StringValue,
                        LastName = attributes.GetValueOrDefault("sn")?.StringValue,
                        Roles = GetExternalRoles(attributes.GetValueOrDefault("memberOf")?.StringValueArray)
                    };
                    
                    result.Add(externalUser);
                }
                catch (LdapException)
                {
                    break;
                }
            }

            return result;
        }

        private string ExtractGroupName(string str) =>
            str.Split(',')[0];

        private List<ExternalRole> GetExternalRoles(string[] memberOf) =>
            memberOf == null ?
                new List<ExternalRole>() :
                memberOf.Select(s => new ExternalRole { Name = ExtractGroupName(s) }).ToList();
    }
}
