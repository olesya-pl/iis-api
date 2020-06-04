using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.FeatureProcessors
{   
    /// <summary>
    /// Defines FeatureProcessor
    /// </summary>
    public interface IFeatureProcessor
    {
        JObject ProcessMetadata(JObject metadata);
    }
}