using System.Linq;
using System.Collections.Generic;

namespace Iis.Api.GraphQL.Graph
{
    public class GraphResponse
    {
        public IReadOnlyCollection<GraphLink> Links { get; set; }
        public IReadOnlyCollection<GraphNode> Nodes { get; set; }

        public GraphResponse(IReadOnlyCollection<GraphLink> linkList, IReadOnlyCollection<GraphNode> nodeList)
        {
            Links = linkList;
            Nodes = nodeList;
        }
    }
}