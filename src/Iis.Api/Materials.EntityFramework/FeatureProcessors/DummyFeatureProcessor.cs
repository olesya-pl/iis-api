using IIS.Core.Materials.FeatureProcessors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class DummyFeatureProcessor : IFeatureProcessor
    {
        public JObject ProcessMetadata(JObject metadata)
        {
            return metadata;
        }
    }

}