using Iis.Interfaces.Users;
using Iis.Services.Contracts.ExternalUserServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IExternalUserService
    {
        UserSource GetUserSource();
        List<ExternalUser> GetUsers();
    }
}
