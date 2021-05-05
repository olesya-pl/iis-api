using Iis.Api.Configuration;
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
            if (configuration.ServiceType.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                return new DevUserService();
            }

            return null;
        }
    }
}
