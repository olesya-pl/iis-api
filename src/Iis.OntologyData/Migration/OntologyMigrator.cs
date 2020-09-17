using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IMigrationResult Migrate(IMigrationOptions migrationOptions)
        {
            Log($"Міграція {_migration.Title}");
            foreach (var migrationEntity in _migration.GetItems())
            {
                Migrate(migrationEntity, migrationOptions);
            }

            Log($"Міграція посилань");
            foreach (var migrationReference in _migration.GetReferences())
            {
                MigrateReference(migrationReference);
            }

            return new MigrationResult { Log = _log.ToString() };
        }
        public Dictionary<Guid, Guid> Migrate(IMigrationEntity migrationEntity, IMigrationOptions migrationOptions)
        {
            Log($"Міграція {migrationEntity.SourceEntityName} у {migrationEntity.TargetEntityName}");
            var sourceNodes = _data.GetEntitiesByTypeName(migrationEntity.SourceEntityName);
            Log($"Всього {sourceNodes.Count} об'єктів");

            var targetType = _schema.GetEntityTypeByName(migrationEntity.TargetEntityName);

            if (migrationOptions.SaveNewObjects)
            {
                foreach (var node in sourceNodes)
                {
                    var entity = _data.CreateNode(targetType.Id, Guid.NewGuid());
                    hierarchyMapper[node.Id] = entity.Id;
                }
            }

            foreach (var node in sourceNodes)
            {
                if (!_migratedIds.Contains(node.Id))
                {
                    if (migrationOptions.SaveNewObjects)
                    {
                        Migrate(node, migrationEntity);
                    }
                    if (migrationOptions.DeleteOldObjects)
                    {
                        _data.SetNodeIsArchived(node.Id);
                    }
                }
            }

            return hierarchyMapper;
        }
        private void ProcessLinkedEntities(List<string> linkedEntities, IDotNameValues dotNameValues)
        {
            foreach (var linkedDotName in linkedEntities)
            {
                var values = dotNameValues.GetValues(linkedDotName);
                foreach (var value in values)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var linkedId = Guid.Parse(value);
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
                var migrationItems = new List<IMigrationItem>(migrationEntity.GetItems(dotNameValue.DotName));
                if (migrationItems.Count == 0)
                {
                    migrationItems.Add(new MigrationItem(dotNameValue.DotName, dotNameValue.DotName));
                }

                foreach (var migrationItem in migrationItems)
                {
                    SetValue(dotNameValue, dotNameValues, migrationItem?.Options, migrationItem.TargetDotName, entity);
                }
            }
        }
        private void SetValue(IDotNameValue dotNameValue, IDotNameValues dotNameValues, 
            IMigrationItemOptions options, string targetDotName, NodeData entity)
        {
            var value = dotNameValue.Value;
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
                    value = node.GetDotNameValues().GetSingleValue(options.TakeValueFrom);
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
        private void MigrateReference(IMigrationReference migrationReference)
        {
            Log($"{migrationReference.EntityName}: {migrationReference.SourceDotName} => {migrationReference.TargetDotName}");
            var entities = _data.GetEntitiesByTypeName(migrationReference.EntityName);
            var options = new MigrationItemOptions { IsHierarchical = true };
            foreach (var entity in entities)
            {
                Log($"{migrationReference.EntityName} id = {entity.Id}");
                var dotNameValues = entity.GetDotNameValues();
                var items = dotNameValues.GetItems(migrationReference.SourceDotName);
                foreach (var dotNameValue in items)
                {
                    var nodeData = _data.GetNode(entity.Id);
                    SetValue(dotNameValue, null, options, migrationReference.TargetDotName, nodeData);
                    _data.SetNodesIsArchived(dotNameValue.Nodes.Select(n => n.Id));
                }
            }
        }

        private void Log(string s)
        {
            _log.AppendLine(s);
        }
    }
}
