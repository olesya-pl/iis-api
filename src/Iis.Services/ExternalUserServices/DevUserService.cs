using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.ExternalUserServices
{
    public class DevUserService : IExternalUserService
    {
        public List<ExternalUser> GetUsers()
        {
            return new List<ExternalUser>
            {
                new ExternalUser
                {
                    AccountName = "ExternalUser1",
                    Roles = new List<ExternalRole>
                    {
                        new ExternalRole { Name = "ExternalRole1"},
                        new ExternalRole { Name = "ExternalRole2"}
                    }
                },
                new ExternalUser
                {
                    AccountName = "ExternalUser2",
                    Roles = new List<ExternalRole>
                    {
                        new ExternalRole { Name = "ExternalRole1"},
                        new ExternalRole { Name = "ExternalRole3"}
                    }
                },
            };
        }
    }
}
