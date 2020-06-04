using IIS.Core.Materials.FeatureProcessors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class GSMFeatureProcessor : IFeatureProcessor
    {
        public JObject ProcessMetadata(JObject metadata)
        {
            return metadata;
        }
    }

}