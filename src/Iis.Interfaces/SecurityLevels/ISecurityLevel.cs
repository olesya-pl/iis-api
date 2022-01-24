using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public interface ISecurityLevel
    {
        public Guid Id { get; }
        public string Name { get; }
        public int UniqueIndex { get; }
        public int? ParentUniqueIndex { get; }
        public ISecurityLevel Parent { get; }
        public IReadOnlyList<ISecurityLevel> Children { get; }
        public bool IsNew { get; }
    }
}
