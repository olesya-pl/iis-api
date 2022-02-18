using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Ontology;
using Newtonsoft.Json.Linq;

namespace Iis.Services.Contracts.Interfaces
{
    public interface INodeJsonService
    {
        JObject GetJObject(INode node, GetEntityOptions options);
    }
}