using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaNodeType: INodeType, INodeTypeLinked
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Kind Kind { get; set; }
        public bool IsAbstract { get; set; }
        public IReadOnlyList<IRelationTypeLinked> IncomingRelations { get; set; }
        public IReadOnlyList<IRelationTypeLinked> OutgoingRelations { get; set; }
        public IAttributeType AttributeType { get; set; }
        public IRelationTypeLinked RelationType { get; set; }
    }
}
