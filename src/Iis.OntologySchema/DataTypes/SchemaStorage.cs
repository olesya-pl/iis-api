﻿using AutoMapper;
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

        public void Initialize(IOntologyRawData ontologyRawData)
        {
            NodeTypes = ontologyRawData.NodeTypes.ToDictionary(nt => nt.Id, nt => _mapper.Map<SchemaNodeType>(nt));
            RelationTypes = ontologyRawData.RelationTypes.ToDictionary(r => r.Id, r => _mapper.Map<SchemaRelationType>(r));
            AttributeTypes = ontologyRawData.AttributeTypes.ToDictionary(at => at.Id, at => _mapper.Map<SchemaAttributeType>(at));
            foreach (var relationType in RelationTypes.Values)
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
            }
            foreach (var attributeType in AttributeTypes.Values)
            {
                NodeTypes[attributeType.Id].AttributeType = attributeType;
            }
        }

        internal IEnumerable<SchemaNodeTypeRaw> GetNodeTypesRaw()
        {
            return NodeTypes.Values.Select(nt => _mapper.Map<SchemaNodeTypeRaw>(nt));
        }

        internal IEnumerable<SchemaRelationTypeRaw> GetRelationTypesRaw()
        {
            return RelationTypes.Values.Select(r => _mapper.Map<SchemaRelationTypeRaw>(r));
        }

        internal IEnumerable<SchemaAttributeTypeRaw> GetAttributeTypesRaw()
        {
            return AttributeTypes.Values.Select(at => _mapper.Map<SchemaAttributeTypeRaw>(at));
        }

        internal Dictionary<string, INodeTypeLinked> GetStringCodes()
        {
            return NodeTypes.Values.Where(nt => nt.GetStringCode() != null).ToDictionary(nt => nt.GetStringCode(), nt => (INodeTypeLinked)nt);
        }
    }
}
