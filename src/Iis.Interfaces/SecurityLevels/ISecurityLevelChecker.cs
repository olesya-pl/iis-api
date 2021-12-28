using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public interface ISecurityLevelChecker
    {
        bool AccessGranted(IReadOnlyList<ISecurityLevel> userLevels, IReadOnlyList<ISecurityLevel> objectLevels);
    }
}
