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
        public OntologySchema(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void Initialize(IEnumerable<INodeType> nodeTypes, IEnumerable<IRelationType> relationTypes, IEnumerable<IAttributeType> attributeTypes)
        {
            _storage = new SchemaStorage(_mapper);
            _storage.Initialize(nodeTypes, relationTypes, attributeTypes);
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

        public void AddNodeType(INodeType nodeType)
        {
            var schemaNodeType = new SchemaNodeType();
            schemaNodeType.CopyFrom(nodeType);
            schemaNodeType.Id = nodeType.Id == default ? Guid.NewGuid() : nodeType.Id;
            _storage.NodeTypes[schemaNodeType.Id] = schemaNodeType;
        }
    }
}
