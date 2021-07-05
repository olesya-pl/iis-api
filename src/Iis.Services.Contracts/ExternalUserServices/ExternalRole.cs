using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.ExternalUserServices
{
    public class ExternalRole
    {
        public string Name { get; set; }
        public override string ToString() => Name;
    }
}
