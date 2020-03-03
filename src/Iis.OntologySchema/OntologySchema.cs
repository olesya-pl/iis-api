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
        public OntologySchema()
        {
            _mapper = GetMapper();
        }

        public static OntologySchema GetInstance(IOntologyRawData ontologyRawData)
        {
            var schema = new OntologySchema();
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
                .Select(key => new SchemaCompareDiffItem { NewNode = thisCodes[key], OldNode = thisCodes[key] })
                .ToList();
            return result;
        }
    }
}