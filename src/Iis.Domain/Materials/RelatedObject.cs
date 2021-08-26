using System;
namespace Iis.Domain.Materials
{
    public class RelatedObject
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string NodeType { get; set; }
        public string RelationType { get; set; }
        public string RelationCreatingType { get; set; }
    }
}