using System;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.FeatureProcessors
{
    public record ProcessingMaterialEntry
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public JObject Metadata { get; set; }
    }
}