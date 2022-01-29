﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.SecurityLevels
{
    public interface ISecurityLevelChecker
    {
        ISecurityLevel RootLevel { get; }
        void Reload();
        ISecurityLevel GetSecurityLevel(Guid id);
        ISecurityLevel GetSecurityLevel(int uniqueIndex);
        IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<Guid> securityLevelIds);
        IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<int> securityLevelIndexes);
        IReadOnlyList<int> GetSecurityLevelIndexes(IReadOnlyList<Guid> securityLevelIds);
        IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain();
        ISecurityLevel GetSecurityLevelByName(string name);
        ISecurityLevel CreateChildLevel(int parentIndex);
        bool AccessGranted(IReadOnlyList<int> userIndexes, IReadOnlyList<int> objectIndexes);
        string GetStringCode(bool includeAll, IReadOnlyList<int> indexes);
        bool IsNameUnique(Guid id, string name);
    }
}
