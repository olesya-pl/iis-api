using AutoMapper;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData
{
    public class OntologyNodesData
    {
        DataStorage _storage;
        IMapper _mapper;
        IOntologySchema _schema;

        public IOntologyPatch Patch => _storage.Patch;

        public OntologyNodesData(INodesRawData rawData, IOntologySchema schema)
        {
            _mapper = GetMapper();
            _schema = schema;
            _storage = new DataStorage(rawData, _mapper, _schema);
        }
        private IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<INodeBase, NodeData>();
                cfg.CreateMap<IRelationBase, RelationData>();
                cfg.CreateMap<IAttributeBase, AttributeData>();
            });

            return new Mapper(configuration);
        }

        public void Migrate(IMigrationEntity migrationEntity)
        {
            var sourceNodes = _storage.Nodes.Values
                .Where(n => n.NodeType.Kind == Kind.Entity
                    && n.NodeType.Name == migrationEntity.SourceEntityName).ToList();

            foreach (var node in sourceNodes)
            {
                Migrate(node, migrationEntity);
                break;
            }
        }
        private void Migrate(NodeData sourceNode, IMigrationEntity migrationEntity)
        {
            var dotNameValues = sourceNode.GetDotNameValues();
            var targetType = _schema.GetEntityTypeByName(migrationEntity.TargetEntityName);
            var entity = _storage.CreateNode(targetType.Id);
            foreach (var dotNameValue in dotNameValues.Items)
            {
                var migrationItem = migrationEntity.GetItem(dotNameValue.DotName);
                var targetDotName = migrationItem?.TargetDotName ?? dotNameValue.DotName;
                var options = migrationItem?.Options;
                if (options != null)
                {
                    if (options.Ignore) continue;

                    if (!string.IsNullOrEmpty(options.IgnoreIfFieldsAreNotEmpty) &&
                        dotNameValues.ContainsOneOf(options.IgnoreIfFieldsAreNotEmpty.Split(',')))
                    {
                        continue;
                    }
                }
                AddValueByDotName(entity, dotNameValue.Value, targetDotName.Split('.'));
            }
        }
        private void AddValueByDotName(NodeData entity, string value, string[] dotNameParts)
        {
            var node = entity;
            for (int i = 0; i < dotNameParts.Length; i++)
            {
                var isLastItem = i == dotNameParts.Length - 1;

                var relationType = node.NodeType.GetRelationTypeByName(dotNameParts[i]);
                if (relationType == null)
                {
                    throw new Exception($"Cannot find embedded type {dotNameParts[i]} for entity type {node.NodeType.Name}");
                }

                if (!isLastItem)
                {
                    var existingRelation = entity._outgoingRelations
                        .SingleOrDefault(r => r.Node.NodeTypeId == relationType.Id);
                    if (existingRelation != null)
                    {
                        node = existingRelation._targetNode;
                        continue;
                    }
                }

                if (isLastItem && relationType.TargetType.IsSeparateObject)
                {
                    var linkNode = _storage.CreateNode(relationType.Id);
                    _storage.CreateRelation(linkNode.Id, node.Id, Guid.Parse(value));
                    break;
                }

                var relationNode = _storage.CreateNode(relationType.Id);
                var targetNode = _storage.CreateNode(relationType.TargetTypeId);
                _storage.CreateRelation(relationNode.Id, node.Id, targetNode.Id);

                if (isLastItem)
                {
                    _storage.CreateAttribute(targetNode.Id, value);
                }

                node = targetNode;
            }
        }
        public void ClearPatch()
        {
            _storage.ClearPatch(); 
        }
    }
}