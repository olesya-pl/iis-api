using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.FeatureProcessors
{   
    /// <summary>
    /// Defines FeatureProcessor
    /// </summary>
    public interface IFeatureProcessor
    {
        Task<JObject> ProcessMetadata(JObject metadata);
    }
}