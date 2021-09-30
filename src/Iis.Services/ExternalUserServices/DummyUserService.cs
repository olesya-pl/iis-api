using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using System.Collections.Generic;

namespace Iis.Services.ExternalUserServices
{
    public class DummyUserService : IExternalUserService
    {
        private readonly Dictionary<string, ExternalUser> _users = new Dictionary<string, ExternalUser>
        {
            {
                "ExternalUser1",
                new ExternalUser
                {
                    UserName = "ExternalUser1",
                    Roles = new List<ExternalRole>
                    {
                        ExternalRole.CreateFrom("Оператор"),
                        ExternalRole.CreateFrom("Аналітик 1")
                    }
                }
            },
            {
                "ExternalUser2",
                new ExternalUser
                {
                    UserName = "ExternalUser2",
                    Roles = new List<ExternalRole>
                    {
                        ExternalRole.CreateFrom("Оператор"),
                        ExternalRole.CreateFrom("Аналітик 1")
                    }
                }
            }
        };

        public UserSource GetUserSource() => UserSource.Dummy;

        public IEnumerable<ExternalUser> GetUsers()
        {
            return _users.Values;
        }

        public bool ValidateCredentials(string username, string password) => password == "123";

        public ExternalUser GetUser(string username)
        {
            _users.TryGetValue(username, out var user);
            return user;
        }
    }
}