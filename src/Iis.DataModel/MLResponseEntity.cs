using System;

namespace Iis.DataModel
{
    public class MLResponseEntity : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public String MLHandlerName { get; set; }
        public String OriginalResponse { get; set; }
    }
}
