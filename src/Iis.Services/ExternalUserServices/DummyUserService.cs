using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.ExternalUserServices
{
    public class DummyUserService : IExternalUserService
    {
        public UserSource GetUserSource() => UserSource.Dummy;
        public List<ExternalUser> GetUsers()
        {
            return new List<ExternalUser>
            {
                new ExternalUser
                {
                    UserName = "ExternalUser1",
                    Roles = new List<ExternalRole>
                    {
                        new ExternalRole { Name = "Оператор"},
                        new ExternalRole { Name = "Аналітик 1"}
                    }
                },
                new ExternalUser
                {
                    UserName = "ExternalUser2",
                    Roles = new List<ExternalRole>
                    {
                        new ExternalRole { Name = "Оператор"},
                        new ExternalRole { Name = "Аналітик 2"}
                    }
                },
            };
        }
        public bool ValidateCredentials(string username, string password) => password == "123";
    }
}
