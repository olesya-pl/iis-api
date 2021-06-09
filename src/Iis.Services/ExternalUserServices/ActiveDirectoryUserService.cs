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
        private const string DistinguishedName = "DC=contour,DC=com";
        private const int DefaultPort = 389;
        private static readonly string[] IncludedFields = new string[] { "name", "objectGUID" };
        public ActiveDirectoryUserService(ExternalUserServiceConfiguration configuration)
        {
            _configuration = configuration;
        }
        public UserSource GetUserSource() => UserSource.ActiveDirectory;
        private LdapConnection GetLdapConnection()
        {
            var connection = new LdapConnection();
            connection.Connect(_configuration.Server, DefaultPort);
            connection.Bind(_configuration.Username, _configuration.Password);
            return connection;
        }
        public List<ExternalUser> GetUsers()
        {
            var myDomainUsers = GetAllUsers();

                        //try
                        //{
                        //    var groups = domainUser.GetGroups(ctx);
                        //    foreach (Principal p in groups)
                        //    {
                        //        if (p is GroupPrincipal)
                        //        {
                        //            var externalRole = new ExternalRole
                        //            {
                        //                Name = (p as GroupPrincipal).Name
                        //            };
                        //            externalUser.Roles.Add(externalRole);
                        //        }
                        //    }
                        //}
                        //catch { }

            return myDomainUsers;
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

        private ILdapSearchResults MakeRequest(LdapConnection connection, string filter)
        {
            return connection.Search(DistinguishedName, LdapConnection.ScopeSub, filter, IncludedFields, false);
        }

        private List<ExternalUser> GetAllUsers()
        {
            using var ldapConn = GetLdapConnection();

            var response = MakeRequest(ldapConn, "(objectClass=user)");
            var result = new List<ExternalUser>();

            while (response.HasMore())
            {
                try
                {
                    LdapEntry nextEntry = response.Next();
                    var attributes = nextEntry.GetAttributeSet();
                    result.Add(new ExternalUser
                    {
                        UserName = attributes["name"].StringValue
                    });
                }
                catch (LdapException)
                {
                    break;
                }
            }

            return result;
        }
    }
}
