using Iis.Api.Configuration;
using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Authentication;
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
        public List<ExternalUser> GetUsers()
        {
            var myDomainUsers = new List<ExternalUser>();

            using (var ctx = GetPrincipalContext())
            {
                var userPrincipal = new UserPrincipal(ctx);

                using (var search = new PrincipalSearcher(userPrincipal))
                {
                    foreach (UserPrincipal domainUser in search.FindAll())
                    {
                        var externalUser = new ExternalUser
                        {
                            UserName = domainUser.SamAccountName,
                            FirstName = domainUser.GivenName,
                            SecondName = domainUser.MiddleName,
                            LastName = domainUser.Surname,
                            Roles = new List<ExternalRole>()
                        };

                        var groups = domainUser.GetAuthorizationGroups();
                        foreach (Principal p in groups)
                        {
                            if (p is GroupPrincipal)
                            {
                                var externalRole = new ExternalRole
                                {
                                    Name = (p as GroupPrincipal).Name
                                };
                                externalUser.Roles.Add(externalRole);
                            }
                        }

                        myDomainUsers.Add(externalUser);
                    }
                }
            }

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
    }
}
