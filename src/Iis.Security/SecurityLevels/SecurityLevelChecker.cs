using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Security.SecurityLevels
{
    public class SecurityLevelChecker : ISecurityLevelChecker
    {
        public bool AccessGranted(IReadOnlyList<ISecurityLevel> userLevels, IReadOnlyList<ISecurityLevel> objectLevels)
        {
            return true;
        }
    }
}
