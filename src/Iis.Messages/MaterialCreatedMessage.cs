using System;

namespace Iis.Messages
{
    public class MaterialRabbitConsts
    {
        public const string QueueName = "material.created";
    }

    public class MaterialCreatedMessage
    {
        public Guid MaterialId { get; set; }
        public Guid? FileId { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
    }
}