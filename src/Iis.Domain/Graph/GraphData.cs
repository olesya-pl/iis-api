using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Domain.Graph
{
    public class GraphData
    {
        private List<GraphLink> _linkList = new List<GraphLink>();
        public IReadOnlyCollection<GraphLink> LinkList => _linkList;
        private List<GraphNode> _nodeList = new List<GraphNode>();
        public IReadOnlyCollection<GraphNode> NodeList => _nodeList;

        public GraphData() { }
        public GraphData(IEnumerable<GraphLink> linkList, IEnumerable<GraphNode> nodeList)
        {
            _linkList = linkList.ToList();
            _nodeList = nodeList.ToList();
        }

        public void AddLink(GraphLink link) => _linkList.Add(link);
        public void AddNode(GraphNode node) => _nodeList.Add(node);
        public void AddLinks(IReadOnlyCollection<GraphLink> linkList) => _linkList.AddRange(linkList);
        public void AddNodes(IReadOnlyCollection<GraphNode> nodeList) => _nodeList.AddRange(nodeList);
        public void AddData(GraphData graphData)
        {
            AddLinks(graphData.LinkList);
            AddNodes(graphData.NodeList);
        }
    }
}
