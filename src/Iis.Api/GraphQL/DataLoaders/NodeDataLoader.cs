using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.DataLoaders
{
    public class NodeDataLoader : DataLoaderBase<Tuple<Guid, INodeTypeLinked>, Node>
    {
        private readonly IOntologyService _ontologyService;

        public NodeDataLoader(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }

        protected override ValueTask FetchAsync(IReadOnlyList<Tuple<Guid, INodeTypeLinked>> keys, Memory<Result<Node>> results, CancellationToken cancellationToken)
        {
            var nodeIds = keys.Select(k => k.Item1).ToArray();
            var relationTypes = keys.All(k => k.Item2 != null)
                ? keys.GroupBy(k => k.Item2.Id).Select(g => g.First().Item2).ToArray()
                : null;
            var nodes = _ontologyService.LoadNodes(nodeIds, relationTypes);
            var nodesDict = nodes.ToDictionary(n => n.Id);
            results = new Memory<Result<Node>>(nodeIds.Select(id => (Result<Node>)nodesDict.GetOrDefault(id)).ToArray());
            return default;
        }
    }
}
