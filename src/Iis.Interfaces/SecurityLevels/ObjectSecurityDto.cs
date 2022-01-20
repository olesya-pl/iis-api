using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public class ObjectSecurityDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public IReadOnlyList<int> SecurityIndexes { get; set; }
    }
}
