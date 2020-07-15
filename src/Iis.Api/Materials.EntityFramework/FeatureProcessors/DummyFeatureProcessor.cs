using System.Threading.Tasks;
using IIS.Core.Materials.FeatureProcessors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class DummyFeatureProcessor : IFeatureProcessor
    {
        public Task<JObject> ProcessMetadata(JObject metadata)
        {
            return Task.FromResult(metadata);
        }
    }

}