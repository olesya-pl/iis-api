using System;

namespace Iis.Domain.MachineLearning
{
    public class MLResponse
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string HandlerName { get; set; }
        public string HandlerCode { get; set; }
        public string HandlerVersion { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string OriginalResponse { get; set; }
    }
}