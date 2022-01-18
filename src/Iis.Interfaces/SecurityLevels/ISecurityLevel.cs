using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public interface ISecurityLevel : ISecurityLevelPlain
    {
        public ISecurityLevel Parent { get; }
        public IReadOnlyList<ISecurityLevel> Children { get; }
    }
}
