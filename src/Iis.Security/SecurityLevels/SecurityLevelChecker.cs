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
        private IOntologyNodesData _ontologyData;

        public SecurityLevelChecker() { }

        public SecurityLevelChecker(IOntologyNodesData ontologyData)
        {
            _ontologyData = ontologyData;
            Initialize();
        }

        internal SecurityLevelChecker(SecurityLevel rootLevel)
        {
            _rootLevel = rootLevel;
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

        public void Reload() => Initialize();
        public ISecurityLevel GetSecurityLevel(Guid id) => _rootLevel.GetAllItems().Single(_ => _.Id == id);
        public ISecurityLevel GetSecurityLevel(int uniqueIndex) => _rootLevel.GetAllItems().Single(_ => _.UniqueIndex == uniqueIndex);
        public IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<Guid> securityLevelIds) =>
            _rootLevel.GetAllItems().Where(_ => securityLevelIds.Contains(_.Id)).ToList();
        public IReadOnlyList<ISecurityLevel> GetSecurityLevels(IReadOnlyList<int> securityLevelIndexes) =>
            _rootLevel.GetAllItems().Where(_ => securityLevelIndexes.Contains(_.UniqueIndex)).ToList();
        public IReadOnlyList<int> GetSecurityLevelIndexes(IReadOnlyList<Guid> securityLevelIds) =>
            GetSecurityLevels(securityLevelIds).Select(_ => _.UniqueIndex).ToList();
        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain() =>
            _rootLevel.GetAllItems().Select(_ => new SecurityLevelPlain(_)).ToList();
        public ISecurityLevel GetSecurityLevelByName(string name) =>
            _rootLevel.GetAllItems().FirstOrDefault(_ => name.Equals(_.Name, StringComparison.OrdinalIgnoreCase));
        public ISecurityLevel CreateChildLevel(int parentIndex)
        {
            var parent = GetSecurityLevelConcrete(parentIndex);
            return new SecurityLevel
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                UniqueIndex = -1,
                _parent = parent
            };
        }
        public bool AccessGranted(IReadOnlyList<int> userIndexes, IReadOnlyList<int> objectIndexes)
        {
            return true;
        }
        /// This is security code for elastic painless script.
        /// See details here: https://confluence.infozahyst.com/pages/viewpage.action?pageId=192484154 
        public string GetStringCode(bool includeAll, params int[] uniqueIndexes) => GetStringCode(includeAll, uniqueIndexes.ToList());
        public string GetStringCode(bool includeAll, IReadOnlyList<int> uniqueIndexes) => GetStringCode(includeAll, _rootLevel.GetAllItems(uniqueIndexes));
        public bool IsNameUnique(Guid id, string name)
        {
            var levelWithTheSameName = GetSecurityLevelByName(name);
            return levelWithTheSameName == null || levelWithTheSameName.Id == id;
        }

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
        internal SecurityLevel GetSecurityLevelConcrete(int uniqueIndex) =>
            _rootLevel.GetAllItems().Where(_ => _.UniqueIndex == uniqueIndex).Single();

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

        private void Initialize()
        {
            var levelsNodes = _ontologyData.GetEntitiesByTypeName(EntityTypeNames.SecurityLevel.ToString());
            var rowLevels = levelsNodes.Select(_ =>
                new
                {
                    SecurityLevel = new SecurityLevel
                    {
                        Id = _.Id,
                        Name = _.GetSingleDirectProperty(OntologyNames.NameField)?.Value,
                        UniqueIndex = int.Parse(_.GetSingleDirectProperty(OntologyNames.UniqueIndexField)?.Value),
                    },
                    ParentIndexStr = _.GetSingleDirectProperty(OntologyNames.ParentField)
                        ?.GetSingleDirectProperty(OntologyNames.UniqueIndexField)?.Value
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
    }
}
