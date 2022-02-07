using System;

namespace Iis.Messages.Materials
{
    public class MaterialNextAssignedMessage
    {
        public Guid UserId { get; set; }
        public Guid? MaterialId { get; set; }
    }
}