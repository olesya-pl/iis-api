using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Configuration
{
    public class ExternalUserServiceConfiguration
    {
        public string ServiceType { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
