using Iis.Api.Configuration;
using Iis.Interfaces.Users;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.ExternalUserServices
{
    public class ExternalUserServiceFactory
    {
        public IExternalUserService GetInstance(ExternalUserServiceConfiguration configuration)
        {
            switch (configuration.ServiceType.ToLower())
            {
                case "ad":
                    return new ActiveDirectoryUserService(configuration);
                case "dummy":
                    return new DummyUserService();
                default:
                    return null;
            }
        }
    }
}
