using System;
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
        bool AccessGranted(IReadOnlyList<ISecurityLevel> userLevels, IReadOnlyList<ISecurityLevel> objectLevels);
        string GetObjectElasticCode(IReadOnlyList<int> indexes);
        string GetUserElasticCode(IReadOnlyList<int> indexes);
        bool IsNameUnique(Guid id, string name);
    }
}
