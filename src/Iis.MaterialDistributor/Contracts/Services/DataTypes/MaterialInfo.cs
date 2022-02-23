using Iis.Messages.Materials;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public class MaterialInfo
    {
        public MaterialInfo(Material material)
        {
            Material = material;
            Metadata = string.IsNullOrWhiteSpace(material.Metadata)
                ? null
                : JObject.Parse(material.Metadata);
        }

        public Material Material { get; }
        public JObject Metadata { get; }
    }
}