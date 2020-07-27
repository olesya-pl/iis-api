using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.Comparison;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Iis.OntologySchema
{
    public class OntologySchema: IOntologySchema
    {
        IMapper _mapper;
        SchemaStorage _storage;
        public IOntologySchemaSource SchemaSource { get; private set; }
        public IAliases Aliases => _storage.Aliases;
        public OntologySchema(IOntologySchemaSource schemaSource)
        {
            SchemaSource = schemaSource;
            _mapper = GetMapper();
            Initialize(new OntologyRawData());
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

        public IEnumerable<INodeTypeLinked> GetEntityTypes()
        {
            return _storage.NodeTypes.Values
                .Where(nt => nt.Kind == Kind.Entity)
                .OrderBy(nt => nt.Name);
        }

        public INodeTypeLinked GetNodeTypeById(Guid id)
        {
            return _storage.NodeTypes.Values
                .Where(nt => nt.Id == id)
                .SingleOrDefault();
        }

        public IOntologyRawData GetRawData()
        {
            return new OntologyRawData(_storage.GetNodeTypesRaw(), _storage.GetRelationTypesRaw(), _storage.GetAttributeTypesRaw(), _storage.GetAliases());
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

            var aliasesComparison = Aliases.CompareTo(schema.Aliases);
            result.AliasesToAdd = aliasesComparison.itemsToAdd.ToList();
            result.AliasesToDelete = aliasesComparison.itemsToDelete.ToList();
            result.AliasesToUpdate = aliasesComparison.itemsToUpdate.ToList();

            return result;
        }
        
        private INodeTypeLinked UpdateNodeType(SchemaNodeType nodeType, INodeTypeUpdateParameter updateParameter)
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

            nodeType.UniqueValueFieldName = updateParameter.UniqueValueFieldName;

            _storage.Aliases.Update(nodeType.Name, updateParameter.Aliases);
            return nodeType;
        }
        private INodeTypeLinked UpdateRelationNodeType(SchemaNodeType nodeType, INodeTypeUpdateParameter updateParameter)
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
            return nodeType;
        }
        private INodeTypeLinked CreateEntityNodeType(INodeTypeUpdateParameter updateParameter)
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
            return nodeType;
        }
        private INodeTypeLinked CreateAttributeNodeType(INodeTypeUpdateParameter updateParameter)
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
            return attributeNodeType;
        }
        private INodeTypeLinked CreateRelationToEntity(INodeTypeUpdateParameter updateParameter)
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
            return relationNodeType;
        }
        private INodeTypeLinked CreateNodeType(INodeTypeUpdateParameter updateParameter)
        {
            if (updateParameter.EmbeddingOptions == null)
            {
                return CreateEntityNodeType(updateParameter);
            }
            else if (updateParameter.ScalarType != null)
            {
                return CreateAttributeNodeType(updateParameter);
            }
            else if (updateParameter.TargetTypeId != null)
            {
                return CreateRelationToEntity(updateParameter);
            }
            else
            {
                throw new ArgumentException("Bad updateParameter");
            }
        }
        public INodeTypeLinked UpdateNodeType(INodeTypeUpdateParameter updateParameter)
        {
            ValidateNodeTypeUpdateParameter(updateParameter);
            if (updateParameter.Id == null)
            {
                return CreateNodeType(updateParameter);
            }
            var nodeType = _storage.GetNodeTypeById((Guid)updateParameter.Id);
            
            switch (nodeType.Kind)
            {
                case Kind.Entity:
                    return UpdateNodeType(nodeType, updateParameter);
                case Kind.Relation:
                    return UpdateRelationNodeType(nodeType, updateParameter);
                case Kind.Attribute:
                    return UpdateRelationNodeType(nodeType._relationType._nodeType, updateParameter);
            }
            return null;
        }
        private void ValidateNodeTypeUpdateParameter(INodeTypeUpdateParameter updateParameter)
        {
            var regex = new Regex("^[A-Za-z0-9_]+$");
            if (!regex.IsMatch(updateParameter.Name))
            {
                throw new Exception("Имя должно состоять из букв, цифр и символа подчеркивания");
            }
            if (updateParameter.Id == null && updateParameter.ParentTypeId != null 
                && GetNodeTypeById((Guid)updateParameter.ParentTypeId).GetAllChildren().Any(ch => ch.Name == updateParameter.Name))
            {
                throw new Exception("Поле с таким именем у данного объекта уже существует");
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
        public string GetAlias(string fieldDotName)
        {
            var value = _storage.Aliases.GetItem(fieldDotName)?.Value;
            if (value != null) return value;
            var entityTypeName = fieldDotName.Substring(0, fieldDotName.IndexOf('.'));
            var entity = GetEntityTypeByName(entityTypeName);
            var parents = entity.GetDirectAncestors();
            foreach (var parent in parents)
            {
                var newDotName = parent.Name + fieldDotName.Substring(fieldDotName.IndexOf('.'));
                value = GetAlias(newDotName);
                if (value != null) return value;
            }
            return null;
        }

        public IAttributeInfoList GetAttributesInfo(string entityName)
        {
            var items = new List<AttributeInfoItem>();
            foreach (var key in _storage.DotNameTypes.Keys.Where(key => key.StartsWith(entityName)))
            {
                var nodeType = _storage.DotNameTypes[key];
                if (nodeType.Kind != Kind.Attribute) continue;
                var aliases = GetAlias(key)?.Split(',') ?? null;
                var shortDotName = key.Substring(key.IndexOf('.') + 1);
                var item = new AttributeInfoItem(shortDotName, nodeType.AttributeType.ScalarType, aliases);
                items.Add(item);
            }
            return new AttributeInfo(entityName, items);
        }
        public void RemoveRelation(Guid relationId)
        {
            var relationType = _storage.RelationTypes[relationId];
            if (relationType.TargetType.Kind == Kind.Attribute)
            {
                _storage.RemoveAttributeType(relationType.TargetType.Id);
                _storage.RemoveNodeType(relationType.TargetType.Id);
            }
            _storage.RemoveNodeType(relationType.Id);
            _storage.RemoveRelationType(relationType.Id);
            relationType._sourceType.RemoveRelationType(relationType.Id);
        }
        public IEnumerable<INodeTypeLinked> GetAllNodeTypes()
        {
            return _storage.NodeTypes.Values;
        }
        public void PutInOrder()
        {
            _storage.SetDotNameTypes();
        }
    }
}
