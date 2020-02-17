using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaStorage
    {
        IMapper _mapper;
        public SchemaStorage(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Dictionary<Guid, SchemaNodeType> NodeTypes { get; private set; }

        public void Initialize(IEnumerable<INodeType> nodeTypes, IEnumerable<IRelationType> relationTypes, IEnumerable<IAttributeType> attributeTypes)
        {
            NodeTypes = nodeTypes.ToDictionary(nt => nt.Id, nt => _mapper.Map<SchemaNodeType>(nt));
        }
    }
}
