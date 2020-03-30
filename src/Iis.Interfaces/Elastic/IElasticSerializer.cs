using Newtonsoft.Json.Linq;

using Iis.Interfaces.Ontology;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticSerializer
    {
        string GetJsonByExtNode(IExtNode extNode);
        JObject GetJsonObjectByExtNode(IExtNode extNode, bool IsHeadNode = true);
    }

}