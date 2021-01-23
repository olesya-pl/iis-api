using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Iis.DataModel
{
    public class NodeTypeEntity : BaseEntity, INodeType
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Kind Kind { get; set; }
        public bool IsAbstract { get; set; }
        public string UniqueValueFieldName { get; set; }
        public string IconBase64Body { get; set; }

        public virtual List<RelationTypeEntity> IncomingRelations { get; set; } = new List<RelationTypeEntity>();
        public virtual List<RelationTypeEntity> OutgoingRelations { get; set; } = new List<RelationTypeEntity>();

        public virtual AttributeTypeEntity AttributeType { get; set; }
        public virtual RelationTypeEntity RelationType { get; set; }

        [JsonIgnore]
        public virtual List<NodeEntity> Nodes { get; set; } = new List<NodeEntity>();
    }
}
