using Iis.Services.Contracts.ExternalUserServices;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.ExternalUserServices
{
    public class ActiveDirectoryUserService : IExternalUserService
    {
        public List<ExternalUser> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}
