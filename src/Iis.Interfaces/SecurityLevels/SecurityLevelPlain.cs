using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public class SecurityLevelPlain
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int UniqueIndex { get; set; }
        public int? ParentUniqueIndex { get; set; }
        
        public SecurityLevelPlain() { }

        public SecurityLevelPlain(ISecurityLevel level)
        {
            Id = level.Id;
            Name = level.Name;
            UniqueIndex = level.UniqueIndex;
            ParentUniqueIndex = level.ParentUniqueIndex;
        }
    }
}
