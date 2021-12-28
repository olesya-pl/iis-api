using System;
using System.Collections.Generic;
using System.Text;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Security.SecurityLevels
{
    internal class SecurityLevel: ISecurityLevel
    {
        public Guid Id { get;  set; }
        public string Name { get; set; }
        internal SecurityLevel _parent { get; private set; }
        public ISecurityLevel Parent => _parent;
        internal List<SecurityLevel> _children { get; private set; }
        public IReadOnlyList<ISecurityLevel> Children => _children;
    }
}
