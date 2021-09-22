using System;

namespace Iis.Messages.Materials
{

    public class MaterialCreatedMessage
    {
        public Guid MaterialId { get; set; }
        public Guid? FileId { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Channel { get; set; }
    }
}