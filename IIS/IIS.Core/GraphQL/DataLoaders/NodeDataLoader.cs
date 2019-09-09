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
            var relationTypeIds = keys.All(k => k.Item2 != null) ? keys.Select(k => k.Item2.Id).Distinct().ToArray() : null;
            var nodes = await _ontologyService.LoadNodesAsync(nodeIds, relationTypeIds, cancellationToken);
            var nodesDict = nodes.ToDictionary(n => n.Id);
            return nodeIds.Select(id => (Result<Node>)nodesDict[id]).ToList();
        }
    }
}
