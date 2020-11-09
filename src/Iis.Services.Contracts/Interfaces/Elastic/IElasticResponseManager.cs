using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces.Elastic
{
    public interface IElasticResponseManager
    {
        Task<JObject> GenerateHighlightsWithoutDublications(JObject source, JToken highlights);
    }
}
