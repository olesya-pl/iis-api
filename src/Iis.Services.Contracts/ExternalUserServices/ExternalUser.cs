using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.ExternalUserServices
{
    public class ExternalUser
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public List<ExternalRole> Roles { get; set; } = new List<ExternalRole>();

        public override string ToString() => UserName;
    }
}
