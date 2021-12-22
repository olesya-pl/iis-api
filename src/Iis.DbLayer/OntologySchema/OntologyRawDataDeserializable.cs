using System.Collections.Generic;
using Iis.OntologySchema.DataTypes;

namespace Iis.DbLayer.OntologySchema
{
    public class OntologyRawDataDeserializable
    {
        public List<SchemaNodeTypeRaw> NodeTypes { get; set; }
        public List<SchemaRelationTypeRaw> RelationTypes { get; set; }
        public List<SchemaAttributeTypeRaw> AttributeTypes { get; set; }
        public List<SchemaAlias> Aliases { get; set; }
    }
}
