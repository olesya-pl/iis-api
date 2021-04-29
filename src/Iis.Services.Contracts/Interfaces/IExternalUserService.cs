using Iis.Services.Contracts.ExternalUserServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IExternalUserService
    {
        List<ExternalUser> GetUsers();
    }
}
