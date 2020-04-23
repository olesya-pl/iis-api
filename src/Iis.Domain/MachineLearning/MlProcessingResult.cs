using System;
namespace Iis.Domain.MachineLearning
{
    public class MlProcessingResult
    {
        public Guid Id { get; set; }
        public string MlHandlerName { get; set; }
        public string ResponseText { get; set; }
    }

}