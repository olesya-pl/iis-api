using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Security.SecurityLevels
{
    public class SecurityLevelChecker : ISecurityLevelChecker
    {
        private SecurityLevel _rootLevel;
        internal SecurityLevelChecker(SecurityLevel rootLevel)
        {
            _rootLevel = rootLevel;
        }
        public SecurityLevelChecker(IOntologyNodesData ontologyData)
        {
            const string NAME = "name";
            const string UNIQUE_INDEX = "uniqueIndex";
            const string PARENT = "parent";

            var levelsNodes = ontologyData.GetEntitiesByTypeName(EntityTypeNames.SecurityLevel.ToString());
            var rowLevels = levelsNodes.Select(_ => 
                new
                {
                    SecurityLevel = new SecurityLevel
                    {
                        Id = _.Id,
                        Name = _.GetSingleDirectProperty(NAME)?.Value,
                        UniqueIndex = int.Parse(_.GetSingleDirectProperty(UNIQUE_INDEX)?.Value),
                    },
                    ParentIndexStr = _.GetSingleDirectProperty(PARENT)?.GetSingleDirectProperty(UNIQUE_INDEX)?.Value
                }
            ).ToDictionary(_ => _.SecurityLevel.UniqueIndex);

            foreach (var rowLevel in rowLevels.Values)
            {
                if (!string.IsNullOrEmpty(rowLevel.ParentIndexStr))
                {
                    var parentLevel = rowLevels[int.Parse(rowLevel.ParentIndexStr)].SecurityLevel;
                    rowLevel.SecurityLevel._parent = parentLevel;
                    parentLevel._children.Add(rowLevel.SecurityLevel);
                }
            }
            _rootLevel = rowLevels.Values.First().SecurityLevel.Root;
        }

        public SecurityLevelChecker(IReadOnlyList<SecurityLevelPlain> plainLevels)
        {
            var levelsDict = plainLevels.Select(_ => new SecurityLevel
            {
                Id = _.Id,
                Name = _.Name,
                UniqueIndex = _.UniqueIndex
            }).ToDictionary(_ => _.UniqueIndex);

            foreach (var plainLevel in plainLevels)
            {
                if (plainLevel.ParentUniqueIndex == null) continue;

                var level = levelsDict[plainLevel.UniqueIndex];
                var parentLevel = levelsDict[(int)plainLevel.ParentUniqueIndex];
                level._parent = parentLevel;
                parentLevel._children.Add(level);
            }

            _rootLevel = levelsDict.Values.First().Root;
        }

        public ISecurityLevel RootLevel => _rootLevel;

        public IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<Guid> securityLevelIds) =>
            _rootLevel.GetAllItems().Where(_ => securityLevelIds.Contains(_.Id)).ToList();
        public IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<int> securityLevelIndexes) =>
            _rootLevel.GetAllItems().Where(_ => securityLevelIndexes.Contains(_.UniqueIndex)).ToList();
        public IReadOnlyList<int> GetSecurityLevelIndexes(IReadOnlyList<Guid> securityLevelIds) =>
            GetSecurityLevels(securityLevelIds).Select(_ => _.UniqueIndex).ToList();
        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain() =>
            _rootLevel.GetAllItems().Select(_ => new SecurityLevelPlain(_)).ToList();
            
        public bool AccessGranted(IReadOnlyList<int> userIndexes, IReadOnlyList<int> objectIndexes)
        {
            return true;
        }
        /// This is security code for elastic painless script.
        /// See details here: https://confluence.infozahyst.com/pages/viewpage.action?pageId=192484154 
        public string GetStringCode(bool includeAll, params int[] uniqueIndexes) => GetStringCode(includeAll, uniqueIndexes.ToList());
        public string GetStringCode(bool includeAll, IReadOnlyList<int> uniqueIndexes) => GetStringCode(includeAll, _rootLevel.GetAllItems(uniqueIndexes));
        internal string GetStringCode(bool includeAll, IReadOnlyList<SecurityLevel> baseLevels)
        {
            var sb = new StringBuilder();
            var levels = includeAll ? GetAllAccessibleLevels(baseLevels) : baseLevels;
            
            var grouped = levels
                .Where(_ => !_.IsRoot)
                .GroupBy(_ => _._parent?.UniqueIndex);

            foreach (var group in grouped.OrderBy(_ => _.Key))
            {
                sb.Append($"{group.Key}:{string.Join(',', group.Select(_ => _.UniqueIndex.ToString()))};");
            }
            return sb.ToString();
        }

        private bool AccessGranted(IReadOnlyList<SecurityLevel> userLevels, IReadOnlyList<SecurityLevel> objectLevels)
        {
            return true;
        }
        private bool AccessGranted(IReadOnlyList<SecurityLevel> userLevels, SecurityLevel objectLevel) =>
            userLevels.Any(userLevel => AccessGranted(userLevel, objectLevel));
        private bool AccessGranted(SecurityLevel userLevel, SecurityLevel objectLevel) =>
            userLevel == objectLevel ||
            userLevel.IsParentOf(objectLevel) ||
            userLevel.IsChildOf(objectLevel);

        private IReadOnlyList<SecurityLevel> GetAllAccessibleLevels(IReadOnlyList<SecurityLevel> baseItems) =>
            _rootLevel.GetAllItems().Where(_ => AccessGranted(baseItems, _)).ToList();
    }
}
