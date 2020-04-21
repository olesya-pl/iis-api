using System;

namespace Iis.Domain.MachineLearning
{
    public class MlResponse
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string HandlerName { get; set; }
        public string OriginalResponse { get; set; }
    }
}