using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.Migration
{
    public class OntologyMigrator
    {
        OntologyNodesData _data;
        IOntologySchema _schema;
        IMigration _migration;
        Dictionary<Guid, Guid> hierarchyMapper = new Dictionary<Guid, Guid>();
        HashSet<Guid> _migratedIds = new HashSet<Guid>();
        StringBuilder _log = new StringBuilder();
        public OntologyMigrator(OntologyNodesData data, IOntologySchema schema, IMigration migration)
        {
            _data = data;
            _schema = schema;
            _migration = migration;
        }
        public IMigrationResult Migrate()
        {
            Log($"Міграція {_migration.Title}");
            foreach (var migrationEntity in _migration.GetItems())
            {
                Migrate(migrationEntity);
            }

            return new MigrationResult { Log = _log.ToString() };
        }
        public Dictionary<Guid, Guid> Migrate(IMigrationEntity migrationEntity)
        {
            Log($"Міграція {migrationEntity.SourceEntityName} у {migrationEntity.TargetEntityName}");
            var sourceNodes = _data.GetEntitiesByTypeName(migrationEntity.SourceEntityName);
            Log($"Всього {sourceNodes.Count} об'єктів");

            var targetType = _schema.GetEntityTypeByName(migrationEntity.TargetEntityName);

            foreach (var node in sourceNodes)
            {
                var entity = _data.CreateNode(targetType.Id, Guid.NewGuid());
                hierarchyMapper[node.Id] = entity.Id;
            }

            foreach (var node in sourceNodes)
            {
                if (!_migratedIds.Contains(node.Id))
                {
                    Migrate(node, migrationEntity);
                    //_storage.SetNodeIsArchived(node.Id);
                }
            }

            return hierarchyMapper;
        }
        private void ProcessLinkedEntities(List<string> linkedEntities, IDotNameValues dotNameValues)
        {
            foreach (var linkedDotName in linkedEntities)
            {
                if (dotNameValues.Contains(linkedDotName))
                {
                    var linkedIdStr = dotNameValues.GetValue(linkedDotName);
                    if (!string.IsNullOrEmpty(linkedIdStr))
                    {
                        var linkedId = Guid.Parse(linkedIdStr);
                        var linkedNode = _data.GetNode(linkedId);
                        _migratedIds.Add(linkedId);
                        Log($"Entity{linkedNode.NodeType.Name}.{linkedId}/view");
                    }
                }
            }
        }
        private void Migrate(INode sourceNode, IMigrationEntity migrationEntity)
        {
            var dotNameValues = sourceNode.GetDotNameValues();
            
            _migratedIds.Add(sourceNode.Id);

            var entity = _data.GetNode(hierarchyMapper[sourceNode.Id]);
            Log("");
            Log($"Entity{sourceNode.NodeType.Name}.{sourceNode.Id}/view");
            
            if (migrationEntity.LinkedEntities != null)
            {
                ProcessLinkedEntities(migrationEntity.LinkedEntities, dotNameValues);
            }
            
            Log($"Entity{entity.NodeType.Name}.{entity.Id}/view");

            foreach (var dotNameValue in dotNameValues.Items)
            {
                var migrationItem = migrationEntity.GetItem(dotNameValue.DotName);
                var targetDotNames = new List<string> { migrationItem?.TargetDotName ?? dotNameValue.DotName };
                if (migrationItem?.OtherTargetDotNames != null)
                {
                    targetDotNames.AddRange(migrationItem.OtherTargetDotNames);
                }
                foreach (var targetDotName in targetDotNames)
                {
                    SetValue(dotNameValue, dotNameValues, migrationItem, targetDotName, entity);
                }
            }
        }
        private void SetValue(IDotNameValue dotNameValue, IDotNameValues dotNameValues, 
            IMigrationItem migrationItem, string targetDotName, NodeData entity)
        {
            var value = dotNameValue.Value;
            var options = migrationItem?.Options;
            if (options != null)
            {
                if (options.Ignore) return;

                if (!string.IsNullOrEmpty(options.IgnoreIfFieldsAreNotEmpty) &&
                    dotNameValues.ContainsOneOf(options.IgnoreIfFieldsAreNotEmpty.Split(',')))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(options.TakeValueFrom))
                {
                    var id = Guid.Parse(dotNameValue.Value);
                    var node = _data.GetNode(id);
                    value = node.GetDotNameValues().GetValue(options.TakeValueFrom);
                }

                if (options.IsHierarchical)
                {
                    var selfId = Guid.Parse(dotNameValue.Value);
                    value = hierarchyMapper.ContainsKey(selfId) ? hierarchyMapper[selfId].ToString() : null;
                }
            }

            if (!string.IsNullOrEmpty(value))
            {
                Log($"    {dotNameValue.DotName} => {targetDotName}");
                if (value == dotNameValue.Value)
                {
                    Log($"        {value}");
                }
                else
                {
                    Log($"        {dotNameValue.Value} => {value}");
                }
                _data.AddValueByDotName(entity, value, targetDotName.Split('.'));
            }
        }
        private void Log(string s)
        {
            _log.AppendLine(s);
        }
    }
}
