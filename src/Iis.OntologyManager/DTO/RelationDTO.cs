using System;

namespace Iis.OntologyManager.DTO
{
    public class RelationDTO
    {
        public string TypeName { get; set; }
        public Guid NodeId { get; set; }
        public string NodeTypeName { get; set; }
        public string NodeTitle { get; set; }
        public string NodeUrl { get; set; }
    }
}
