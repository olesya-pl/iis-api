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
        public Dictionary<Guid, SchemaRelationType> RelationTypes { get; private set; }
        public Dictionary<Guid, SchemaAttributeType> AttributeTypes { get; private set; }

        public void Initialize(IEnumerable<INodeType> nodeTypes, IEnumerable<IRelationType> relationTypes, IEnumerable<IAttributeType> attributeTypes)
        {
            NodeTypes = nodeTypes.ToDictionary(nt => nt.Id, nt => _mapper.Map<SchemaNodeType>(nt));
            RelationTypes = relationTypes.ToDictionary(r => r.Id, r => _mapper.Map<SchemaRelationType>(r));
            AttributeTypes = attributeTypes.ToDictionary(at => at.Id, at => _mapper.Map<SchemaAttributeType>(at));
            foreach (var relationType in RelationTypes.Values)
            {
                relationType.NodeType = NodeTypes[relationType.Id];
                relationType.NodeType.RelationType = relationType;
                
                var sourceType = NodeTypes[relationType.SourceTypeId];
                relationType.SourceType = sourceType;
                sourceType.AddOutgoingRelation(relationType);
                
                var targetType = NodeTypes[relationType.TargetTypeId];
                relationType.TargetType = targetType;
                targetType.AddIncomingRelation(relationType);
            }
            foreach (var attributeType in AttributeTypes.Values)
            {
                NodeTypes[attributeType.Id].AttributeType = attributeType;
            }
        }
    }
}
