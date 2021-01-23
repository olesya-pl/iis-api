using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.ChangeParameters
{
    public class NodeTypeUpdateParameter : INodeTypeUpdateParameter
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Meta { get; set; }
        public bool? IsAbstract { get; set; }
        public IEnumerable<string> Aliases { get; set; }
        public EmbeddingOptions? EmbeddingOptions { get; set; }
        public ScalarType? ScalarType { get; set; }
        public Guid? TargetTypeId { get; set; }
        public Guid? ParentTypeId { get; set; }
        public string UniqueValueFieldName { get; set; }
        public string IconBase64Body { get; set; }
    }
}
