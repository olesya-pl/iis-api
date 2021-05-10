using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Core.Tools
{
    public class ExternalUserSeeder
    {
        IExternalUserService _externalUserService;
        public ExternalUserSeeder(IExternalUserService externalUserService)
        {
            _externalUserService = externalUserService;
        }
        public void Seed()
        {
            SeedUsers();
            SeedRoles();
        }
        private void SeedUsers()
        {
            //var externalUsers = 
        }
        private void SeedRoles()
        {

        }
    }
}
