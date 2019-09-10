using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.DataLoaders
{
    public class NodeDataLoader : DataLoaderBase<Tuple<Guid, EmbeddingRelationType>, Node>
    {
        private readonly IOntologyService _ontologyService;

        public NodeDataLoader(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }

        protected override async Task<IReadOnlyList<Result<Node>>> FetchAsync(IReadOnlyList<Tuple<Guid, EmbeddingRelationType>> keys, CancellationToken cancellationToken)
        {
            var nodeIds = keys.Select(k => k.Item1).ToArray();
            var relationTypes = keys.All(k => k.Item2 != null)
                ? keys.GroupBy(k => k.Item2.Id).Select(g => g.First().Item2).ToArray()
                : null;
            var nodes = await _ontologyService.LoadNodesAsync(nodeIds, relationTypes, cancellationToken);
            var nodesDict = nodes.ToDictionary(n => n.Id);
            return nodeIds.Select(id => (Result<Node>)nodesDict.GetOrDefault(id)).ToList();
        }
    }
}
