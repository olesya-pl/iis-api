using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public interface ISecurityLevelPlain
    {
        public Guid Id { get; }
        public string Name { get; }
        public int UniqueIndex { get; }
        public int? ParentUniqueIndex { get; }
    }
}
