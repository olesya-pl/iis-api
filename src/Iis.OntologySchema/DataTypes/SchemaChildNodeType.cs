using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaChildNodeType: IChildNodeType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public Kind Kind { get; set; }
        public Guid RelationId { get; set; }
        public string RelationName { get; set; }
        public string RelationTitle { get; set; }
        public string RelationMeta { get; set; }
        public EmbeddingOptions EmbeddingOptions { get; set; }
    }
}
