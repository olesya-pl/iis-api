using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public class SecurityLevelPlain
    {
        public SecurityLevelPlain() { }

        public SecurityLevelPlain(ISecurityLevel level)
        {
            Id = level.Id;
            Name = level.Name;
            UniqueIndex = level.UniqueIndex;
            ParentUniqueIndex = level.ParentUniqueIndex;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int UniqueIndex { get; set; }
        public int? ParentUniqueIndex { get; set; }
        public bool IsNew => UniqueIndex == -1;

        public override string ToString() => $"{UniqueIndex} : {Name}";
    }
}
