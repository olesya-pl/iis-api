using System.Collections.Generic;
using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IExternalUserService
    {
        UserSource GetUserSource();
        IEnumerable<ExternalUser> GetUsers();
        ExternalUser GetUser(string username);
        bool ValidateCredentials(string username, string password);
    }
}