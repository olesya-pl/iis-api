using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public interface ISecurityLevelChecker
    {
        ISecurityLevel RootLevel { get; }
        IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<Guid> securityLevelIds);
        IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain();
        bool AccessGranted(IReadOnlyList<int> userIndexes, IReadOnlyList<int> objectIndexes);
        string GetStringCode(bool includeAll, IReadOnlyList<int> indexes);
    }
}
