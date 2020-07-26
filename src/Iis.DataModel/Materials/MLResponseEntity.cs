using System;

namespace Iis.DataModel.Materials
{
    public class MLResponseEntity : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public string HandlerName { get; set; }
        public string HandlerVersion { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string OriginalResponse { get; set; }
    }
}
