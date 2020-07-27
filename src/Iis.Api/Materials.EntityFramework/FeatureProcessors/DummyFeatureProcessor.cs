using System.Threading.Tasks;
using IIS.Core.Materials.FeatureProcessors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class DummyFeatureProcessor : IFeatureProcessor
    {
        public bool IsDummy => true;
        public Task<JObject> ProcessMetadataAsync(JObject metadata)
        {
            return Task.FromResult(metadata);
        }
    }

}