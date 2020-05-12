using AutoMapper;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.OntologySchema.DataTypes
{
    internal class SchemaStorage
    {
        IMapper _mapper;
        public SchemaStorage(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Dictionary<Guid, SchemaNodeType> NodeTypes { get; private set; }
        public Dictionary<Guid, SchemaRelationType> RelationTypes { get; private set; }
        public Dictionary<Guid, SchemaAttributeType> AttributeTypes { get; private set; }

        public void Initialize(IOntologyRawData ontologyRawData)
        {
            NodeTypes = ontologyRawData.NodeTypes.ToDictionary(nt => nt.Id, nt => _mapper.Map<SchemaNodeType>(nt));
            RelationTypes = ontologyRawData.RelationTypes.ToDictionary(r => r.Id, r => _mapper.Map<SchemaRelationType>(r));
            AttributeTypes = ontologyRawData.AttributeTypes.ToDictionary(at => at.Id, at => _mapper.Map<SchemaAttributeType>(at));
            foreach (var relationType in RelationTypes.Values)
            {
                relationType.NodeType = NodeTypes[relationType.Id];
                NodeTypes[relationType.Id].SetRelationType(relationType);

                relationType.SourceType = NodeTypes[relationType.SourceTypeId];
                NodeTypes[relationType.SourceTypeId].AddOutgoingRelation(relationType);

                relationType.TargetType = NodeTypes[relationType.TargetTypeId];
                NodeTypes[relationType.TargetTypeId].AddIncomingRelation(relationType);
            }
            foreach (var attributeType in AttributeTypes.Values)
            {
                NodeTypes[attributeType.Id]._attributeType = attributeType;
            }
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

        public Dictionary<string, INodeTypeLinked> GetStringCodes()
        {
            return NodeTypes.Values.Where(nt => nt.GetStringCode() != null).ToDictionary(nt => nt.GetStringCode(), nt => (INodeTypeLinked)nt);
        }

        public SchemaNodeType GetNodeTypeById(Guid id)
        {
            return NodeTypes.Values.SingleOrDefault(nt => nt.Id == id);
        }
    }
}
