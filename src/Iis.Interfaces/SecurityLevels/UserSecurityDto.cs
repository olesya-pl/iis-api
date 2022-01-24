using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public class UserSecurityDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public IReadOnlyList<int> SecurityIndexes { get; set; }
    }
}
