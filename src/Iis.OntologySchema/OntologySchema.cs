using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
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

        private IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<INodeType, SchemaNodeType>();
                cfg.CreateMap<IRelationType, SchemaRelationType>();
                cfg.CreateMap<IAttributeType, SchemaAttributeType>();
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
            return new OntologyRawData(_storage.NodeTypes.Values, _storage.RelationTypes.Values, _storage.AttributeTypes.Values);
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

        
    }
}
