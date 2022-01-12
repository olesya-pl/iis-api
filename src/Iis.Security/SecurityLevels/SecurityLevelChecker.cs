using Iis.Interfaces.Ontology.Data;
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
        internal SecurityLevelChecker(SecurityLevel topLevel)
        {
            _rootLevel = topLevel;
        }
        public SecurityLevelChecker(IOntologyNodesData ontologyData)
        {
            const string NAME = "name";
            const string UNIQUE_INDEX = "uniqueIndex";
            const string PARENT = "parent";

            var levelsNodes = ontologyData.GetEntitiesByTypeName("SecurityLevel");
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
        public IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<Guid> securityLevelIds) =>
            _rootLevel.GetAllItems().Where(_ => securityLevelIds.Contains(_.Id)).ToList();
        public bool AccessGranted(IReadOnlyList<int> userIndexes, IReadOnlyList<int> objectIndexes)
        {
            return true;
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

        // see details here: https://confluence.infozahyst.com/pages/viewpage.action?pageId=192484154
        public string GetStringCode(bool includeAll, params int[] indexes) => GetStringCode(includeAll, indexes.ToList());
        public string GetStringCode(bool includeAll, IReadOnlyList<int> indexes) => GetStringCode(includeAll, _rootLevel.GetAllItems(indexes));
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

        //private SecurityLevel Map(INode node) =>
    }
}
