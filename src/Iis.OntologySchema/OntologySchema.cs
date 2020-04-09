﻿using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.Comparison;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.OntologySchema
{
    public class OntologySchema: IOntologySchema
    {
        IMapper _mapper;
        SchemaStorage _storage;
        public IOntologySchemaSource SchemaSource { get; private set; }
        public OntologySchema(IOntologySchemaSource schemaSource)
        {
            SchemaSource = schemaSource;
            _mapper = GetMapper();
        }

        public static OntologySchema GetInstance(IOntologyRawData ontologyRawData, IOntologySchemaSource schemaSource)
        {
            var schema = new OntologySchema(schemaSource);
            schema.Initialize(ontologyRawData);
            return schema;
        }

        private IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<INodeType, SchemaNodeType>();
                cfg.CreateMap<IRelationType, SchemaRelationType>();
                cfg.CreateMap<IAttributeType, SchemaAttributeType>();
                cfg.CreateMap<INodeType, SchemaNodeTypeRaw>();
                cfg.CreateMap<IRelationType, SchemaRelationTypeRaw>();
                cfg.CreateMap<IAttributeType, SchemaAttributeTypeRaw>();
            });

            return new Mapper(configuration);
        }

        public void Initialize(IOntologyRawData ontologyRawData)
        {
            _storage = new SchemaStorage(_mapper);
            _storage.Initialize(ontologyRawData);
        }

        public IEnumerable<INodeTypeLinked> GetTypes(IGetTypesFilter filter)
        {
            return _storage.NodeTypes.Values
                .Where(nt => (filter.Name == null || nt.Name.ToLower().Contains(filter.Name.ToLower().Trim()))
                    && filter.Kinds.Contains(nt.Kind));
        }

        public INodeTypeLinked GetNodeTypeById(Guid id)
        {
            return _storage.NodeTypes.Values
                .Where(nt => nt.Id == id)
                .SingleOrDefault();
        }

        public IOntologyRawData GetRawData()
        {
            return new OntologyRawData(_storage.GetNodeTypesRaw(), _storage.GetRelationTypesRaw(), _storage.GetAttributeTypesRaw());
        }

        public void AddNodeType(INodeType nodeType)
        {
            var schemaNodeType = new SchemaNodeType();
            schemaNodeType.CopyFrom(nodeType);
            schemaNodeType.Id = nodeType.Id == default ? Guid.NewGuid() : nodeType.Id;
            _storage.NodeTypes[schemaNodeType.Id] = schemaNodeType;
        }

        public void SetEmbeddingOptions(string entityName, string relationName, EmbeddingOptions embeddingOptions)
        {
            var relationType = GetRelationType(entityName, relationName);
            relationType.EmbeddingOptions = embeddingOptions;
        }
        public void SetRelationMeta(string entityName, string relationName, string meta)
        {
            var relationType = GetRelationType(entityName, relationName);
            relationType.SetMeta(meta);
        }

        public Dictionary<string, INodeTypeLinked> GetStringCodes()
        {
            return _storage.GetStringCodes();
        }

        public INodeTypeLinked GetEntityTypeByName(string entityTypeName)
        {
            return _storage.NodeTypes.Values
                .Where(nt => !nt.IsArchived 
                    && nt.Kind == Kind.Entity 
                    && nt.Name == entityTypeName)
                .SingleOrDefault();
        }

        private SchemaRelationType GetRelationType(string entityName, string relationName)
        {
            var relationType = GetRelationTypeOrNull(entityName, relationName);
            if (relationType == null)
            {
                throw new ArgumentException($"There is no relation ({entityName}, {relationName})");
            }
            return relationType;
        }

        private SchemaRelationType GetRelationTypeOrNull(string entityName, string relationName)
        {
            SchemaNodeType nodeType = _storage.NodeTypes.Values
                .Where(nt => nt.Kind == Kind.Entity
                    && nt.Name == entityName).SingleOrDefault();
            return nodeType?.GetRelationByName(relationName);
        }

        public ISchemaCompareResult CompareTo(IOntologySchema schema)
        {
            Dictionary<string, INodeTypeLinked> thisCodes = GetStringCodes();
            Dictionary<string, INodeTypeLinked> otherCodes = schema.GetStringCodes();
            var result = new SchemaCompareResult();
            result.ItemsToAdd = thisCodes.Keys.Where(key => !otherCodes.ContainsKey(key)).Select(key => thisCodes[key]).ToList();
            result.ItemsToDelete = otherCodes.Keys.Where(key => !thisCodes.ContainsKey(key)).Select(key => otherCodes[key]).ToList();
            var commonKeys = thisCodes.Keys.Where(key => otherCodes.ContainsKey(key)).ToList();
            
            result.ItemsToUpdate = commonKeys
                .Where(key => !thisCodes[key].IsIdentical(otherCodes[key]))
                .Select(key => new SchemaCompareDiffItem { NodeTypeFrom = thisCodes[key], NodeTypeTo = otherCodes[key] })
                .ToList();
            result.SchemaSource = schema.SchemaSource;
            return result;
        }
        
        private void UpdateNodeType(SchemaNodeType nodeType, INodeTypeUpdateParameter updateParameter)
        {
            if (!string.IsNullOrEmpty(updateParameter.Title))
            {
                nodeType.Title = updateParameter.Title;
            }

            if (nodeType.Kind == Kind.Relation && !string.IsNullOrEmpty(updateParameter.Meta))
            {
                nodeType.Meta = updateParameter.Meta;
            }

            if (updateParameter.ScalarType != null && nodeType._attributeType != null)
            {
                nodeType._attributeType.ScalarType = (ScalarType)updateParameter.ScalarType;
            }
        }
        private void UpdateRelationNodeType(SchemaNodeType nodeType, INodeTypeUpdateParameter updateParameter)
        {
            UpdateNodeType(nodeType, updateParameter);

            if (updateParameter.EmbeddingOptions != null)
            {
                nodeType._relationType.EmbeddingOptions = (EmbeddingOptions)updateParameter.EmbeddingOptions;
            }

            if (updateParameter.TargetTypeId != null && nodeType.RelationType.TargetType.Kind == Kind.Entity)
            {
                nodeType._relationType.TargetTypeId = (Guid)updateParameter.TargetTypeId;
                nodeType._relationType._targetType = _storage.NodeTypes[(Guid)updateParameter.TargetTypeId];
            }

            UpdateNodeType(nodeType._relationType._targetType, updateParameter);
        }
        private void CreateEntityNodeType(INodeTypeUpdateParameter updateParameter)
        {
            var nodeType = new SchemaNodeType
            {
                Id = Guid.NewGuid(),
                Name = updateParameter.Name,
                Kind = Kind.Entity,
                IsArchived = false,
                IsAbstract = false
            };
            _storage.AddNodeType(nodeType);
            UpdateNodeType(nodeType, updateParameter);
        }
        private void CreateAttributeNodeType(INodeTypeUpdateParameter updateParameter)
        {
            var attributeNodeType = new SchemaNodeType
            {
                Id = Guid.NewGuid(),
                Name = updateParameter.Name,
                Kind = Kind.Attribute,
            };
            _storage.AddNodeType(attributeNodeType);
            var attribute = new SchemaAttributeType 
            { 
                Id = attributeNodeType.Id, 
                ScalarType = (ScalarType)updateParameter.ScalarType 
            };
            _storage.AddAttributeType(attribute);
            var relationNodeType = new SchemaNodeType()
            {
                Id = Guid.NewGuid(),
                Name = updateParameter.Name,
                Kind = Kind.Relation,
            };
            _storage.AddNodeType(relationNodeType);
            var relationType = new SchemaRelationType
            {
                Id = relationNodeType.Id,
                SourceTypeId = (Guid)updateParameter.ParentTypeId,
                TargetTypeId = attributeNodeType.Id
            };
            _storage.AddRelationType(relationType);
            UpdateRelationNodeType(relationNodeType, updateParameter);
        }
        private void CreateRelationToEntity(INodeTypeUpdateParameter updateParameter)
        {
            var relationNodeType = new SchemaNodeType()
            {
                Id = Guid.NewGuid(),
                Name = updateParameter.Name,
                Kind = Kind.Relation,
            };
            _storage.AddNodeType(relationNodeType);
            var relationType = new SchemaRelationType
            {
                Id = relationNodeType.Id,
                SourceTypeId = (Guid)updateParameter.ParentTypeId,
                TargetTypeId = (Guid)updateParameter.TargetTypeId,
            };
            _storage.AddRelationType(relationType);
            UpdateRelationNodeType(relationNodeType, updateParameter);
        }
        private void CreateNodeType(INodeTypeUpdateParameter updateParameter)
        {
            if (updateParameter.EmbeddingOptions == null)
            {
                CreateEntityNodeType(updateParameter);
            }
            else if (updateParameter.ScalarType != null)
            {
                CreateAttributeNodeType(updateParameter);
            }
            else if (updateParameter.TargetTypeId != null)
            {
                CreateRelationToEntity(updateParameter);
            }
            else
            {
                throw new ArgumentException("Bad updateParameter");
            }
        }
        public void UpdateNodeType(INodeTypeUpdateParameter updateParameter)
        {
            if (updateParameter.Id == null)
            {
                CreateNodeType(updateParameter);
                return;
            }
            var nodeType = _storage.GetNodeTypeById((Guid)updateParameter.Id);
            
            switch (nodeType.Kind)
            {
                case Kind.Entity:
                    UpdateNodeType(nodeType, updateParameter);
                    break;
                case Kind.Relation:
                    UpdateRelationNodeType(nodeType, updateParameter);
                    break;
                case Kind.Attribute:
                    UpdateRelationNodeType(nodeType._relationType._nodeType, updateParameter);
                    break;
            }
        }

        public void UpdateTargetType(Guid relationTypeId, Guid targetTypeId)
        {
            var nodeType = _storage.GetNodeTypeById(relationTypeId);
            nodeType._relationType.TargetTypeId = targetTypeId;
            nodeType._relationType._targetType = _storage.NodeTypes[targetTypeId];
        }
        public void SetInheritance(Guid sourceTypeId, Guid targetTypeId)
        {
            var sourceNodeType = _storage.GetNodeTypeById(sourceTypeId);
            if (!sourceNodeType.OutgoingRelations.Any(r => r.TargetTypeId == targetTypeId && r.Kind == RelationKind.Inheritance))
            {
                var relationNodeType = new SchemaNodeType()
                {
                    Id = Guid.NewGuid(),
                    Name = "Is",
                    Kind = Kind.Relation,
                };
                _storage.AddNodeType(relationNodeType);
                var relationType = new SchemaRelationType
                {
                    Id = relationNodeType.Id,
                    SourceTypeId = sourceTypeId,
                    TargetTypeId = targetTypeId,
                    Kind = RelationKind.Inheritance,
                    EmbeddingOptions = EmbeddingOptions.None
                };
                _storage.AddRelationType(relationType);
            }
        }
    }
}
