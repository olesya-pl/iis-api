using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.Ontology
{
    public class OntologyRawDataDeserializable
    {
        public List<SchemaNodeTypeRaw> NodeTypes { get; set; }
        public List<SchemaRelationTypeRaw> RelationTypes { get; set; }
        public List<SchemaAttributeTypeRaw> AttributeTypes { get; set; }
    }
}
