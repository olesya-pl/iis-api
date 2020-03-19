using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaRelationTypeRaw: IRelationType
    {
        public Guid Id { get; set; }
        public RelationKind Kind { get; set; }
        public EmbeddingOptions EmbeddingOptions { get; set; }
        public Guid SourceTypeId { get; set; }
        public Guid TargetTypeId { get; set; }
    }
}
