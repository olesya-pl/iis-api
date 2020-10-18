using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.OntologySchema.DataTypes
{
    internal class SchemaStorage
    {
        IMapper _mapper;
        IOntologySchema _schema;

        public SchemaStorage(IMapper mapper, IOntologySchema schema)
        {
            _mapper = mapper;
            _schema = schema;
        }
        
        public Dictionary<Guid, SchemaNodeType> NodeTypes { get; private set; }
        public Dictionary<Guid, SchemaRelationType> RelationTypes { get; private set; }
        public Dictionary<Guid, SchemaRelationType> InversedRelationTypes { get; private set; } = new Dictionary<Guid, SchemaRelationType>();
        public Dictionary<Guid, SchemaAttributeType> AttributeTypes { get; private set; }
        public SchemaAliases Aliases { get; private set; }
        public Dictionary<string, SchemaNodeType> DotNameTypes { get; private set; } = new Dictionary<string, SchemaNodeType>();
        public IEnumerable<SchemaNodeType> Entities => NodeTypes.Values.Where(nt => !nt.IsArchived && nt.Kind == Kind.Entity);
        public void Initialize(IOntologyRawData ontologyRawData)
        {
            NodeTypes = ontologyRawData.NodeTypes.Where(nt => !nt.IsArchived).ToDictionary(nt => nt.Id, nt => _mapper.Map<SchemaNodeType>(nt));
            RelationTypes = ontologyRawData.RelationTypes.ToDictionary(r => r.Id, r => _mapper.Map<SchemaRelationType>(r));
            AttributeTypes = ontologyRawData.AttributeTypes.ToDictionary(at => at.Id, at => _mapper.Map<SchemaAttributeType>(at));
            Aliases = new SchemaAliases(ontologyRawData.Aliases);
            foreach (var relationId in RelationTypes.Keys)
            {
                var relationType = RelationTypes[relationId];
                if (IsArchived(relationType))
                {
                    RelationTypes.Remove(relationId);
                    continue;
                }

                AddRelation(relationType);
            }

            foreach (var attributeId in AttributeTypes.Keys)
            {
                if (!NodeTypeExists(attributeId))
                {
                    AttributeTypes.Remove(attributeId);
                }
                else
                {
                    NodeTypes[attributeId]._attributeType = AttributeTypes[attributeId];
                }
            }

            foreach (var nodeType in NodeTypes.Values)
            {
                nodeType.Schema = _schema;
            }

            SetDotNameTypes();
        }
        private void AddRelation(SchemaRelationType relationType)
        {
            var nodeType = NodeTypes[relationType.Id];
            relationType.SetNodeType(nodeType);
            nodeType.SetRelationType(relationType);

            var sourceType = NodeTypes[relationType.SourceTypeId];
            relationType.SetSourceType(sourceType);
            sourceType.AddOutgoingRelation(relationType);

            var targetType = NodeTypes[relationType.TargetTypeId];
            relationType.SetTargetType(targetType);
            targetType.AddIncomingRelation(relationType);

            if (nodeType.HasInversed)
            {
                AddInversedRelation(relationType);
            }
        }
        private SchemaRelationType AddInversedRelation(SchemaRelationType directRelationType)
        {
            var inversed = _mapper.Map<SchemaRelationType>(directRelationType);
            var nodeType = _mapper.Map<SchemaNodeType>(NodeTypes[directRelationType.Id]);
            var inversedMeta = directRelationType.NodeType.MetaObject.Inversed;
            nodeType.Id = Guid.NewGuid();
            inversed.Id = nodeType.Id;
            nodeType.Name = inversedMeta.Code ?? directRelationType.SourceType.Name.ToLowerCamelcase();
            nodeType.Title = inversedMeta.Title ?? directRelationType.SourceType.Title ?? nodeType.Name;
            inversed.EmbeddingOptions = inversedMeta.Multiple ? EmbeddingOptions.Multiple : EmbeddingOptions.Optional;
            nodeType.SetIsInversed();
            inversed.SetNodeType(nodeType);
            inversed._directRelationType = directRelationType;
            directRelationType._inversedRelationType = inversed;
            nodeType.SetRelationType(inversed);

            var sourceType = NodeTypes[inversed.TargetTypeId];
            inversed.SetSourceType(sourceType);
            sourceType.AddOutgoingRelation(inversed);

            var targetType = NodeTypes[inversed.SourceTypeId];
            inversed.SetTargetType(targetType);
            targetType.AddIncomingRelation(inversed);

            InversedRelationTypes[inversed.Id] = inversed;
            return inversed;
        }
        public IEnumerable<SchemaNodeTypeRaw> GetNodeTypesRaw()
        {
            return NodeTypes.Values.Select(nt => _mapper.Map<SchemaNodeTypeRaw>(nt));
        }
        public IEnumerable<SchemaRelationTypeRaw> GetRelationTypesRaw()
        {
            return RelationTypes.Values.Select(r => _mapper.Map<SchemaRelationTypeRaw>(r));
        }
        public IEnumerable<SchemaAttributeTypeRaw> GetAttributeTypesRaw()
        {
            return AttributeTypes.Values.Select(at => _mapper.Map<SchemaAttributeTypeRaw>(at));
        }
        public IEnumerable<IAlias> GetAliases()
        {
            return Aliases.Items;
        }
        public Dictionary<string, INodeTypeLinked> GetStringCodes()
        {
            return NodeTypes.Values.Where(nt => nt.GetStringCode() != null).ToDictionary(nt => nt.GetStringCode(), nt => (INodeTypeLinked)nt);
        }
        public SchemaNodeType GetNodeTypeById(Guid id)
        {
            return NodeTypes[id];
        }
        public void AddNodeType(SchemaNodeType nodeType)
        {
            NodeTypes[nodeType.Id] = nodeType;
        }
        public void AddRelationType(SchemaRelationType relationType)
        {
            RelationTypes[relationType.Id] = relationType;
            var nodeType = NodeTypes[relationType.Id];
            if (nodeType != null)
            {
                nodeType._relationType = relationType;
                relationType._nodeType = nodeType;
            }

            var sourceNodeType = NodeTypes[relationType.SourceTypeId];
            if (sourceNodeType != null)
            {
                sourceNodeType.AddOutgoingRelation(relationType);
                relationType._sourceType = sourceNodeType;
            }

            var targetNodeType = NodeTypes[relationType.TargetTypeId];
            if (targetNodeType != null)
            {
                targetNodeType.AddIncomingRelation(relationType);
                relationType._targetType = targetNodeType;
            }
        }
        public void AddAttributeType(SchemaAttributeType attributeType)
        {
            AttributeTypes[attributeType.Id] = attributeType;
            var nodeType = NodeTypes[attributeType.Id];
            if (nodeType != null)
            {
                nodeType._attributeType = attributeType;
            }
        }
        public void RemoveNodeType(Guid id)
        {
            NodeTypes.Remove(id);
        }
        public void RemoveRelationType(Guid id)
        {
            RelationTypes.Remove(id);
        }
        public void RemoveAttributeType(Guid id)
        {
            AttributeTypes.Remove(id);
        }
        public void RemoveEntity(Guid id)
        {
            var nodeType = GetNodeTypeById(id);
            foreach (var relationType in nodeType._outgoingRelations)
            {
                RelationTypes.Remove(relationType.Id);
                NodeTypes.Remove(relationType.Id);
                if (relationType.TargetType.Kind == Kind.Attribute)
                {
                    AttributeTypes.Remove(relationType.TargetType.Id);
                    NodeTypes.Remove(relationType.TargetType.Id);
                }
            }
            NodeTypes.Remove(id);
        }
        public void SetDotNameTypes()
        {
            DotNameTypes.Clear();
            foreach (var nodeType in Entities)
            {
                DotNameTypes[nodeType.Name] = nodeType;
                var childInfos = nodeType.GetNodeTypesRecursive();
                foreach (var childInfo in childInfos)
                {
                    DotNameTypes[childInfo.dotName] = childInfo.nodeType;
                }
            }
        }
        private bool NodeTypeExists(Guid id) => NodeTypes.ContainsKey(id);
        private bool IsArchived(SchemaRelationType relationType)
        {
            return !NodeTypeExists(relationType.Id)
                || !NodeTypeExists(relationType.SourceTypeId)
                || !NodeTypeExists(relationType.TargetTypeId);
        }
    }
}
