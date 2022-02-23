using Iis.Domain.Users;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Ontology;
using Newtonsoft.Json.Linq;

namespace Iis.Services.Contracts.Interfaces
{
    public interface INodeJsonService
    {
        string GetJson(INode node, User user, GetEntityOptions options);
        JObject GetJObject(INode node, User user, GetEntityOptions options);
    }
}