using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.DataLoaders
{
    public class MultipleNodeDataLoader : DataLoaderBase<Tuple<Guid, IEnumerable<IEmbeddingRelationTypeModel>>, Node>
    {
        private readonly IOntologyService _ontologyService;

        public MultipleNodeDataLoader(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }

        protected override async Task<IReadOnlyList<Result<Node>>> FetchAsync(IReadOnlyList<Tuple<Guid, IEnumerable<IEmbeddingRelationTypeModel>>> keys, CancellationToken cancellationToken)
        {
            var nodeIds = keys.Select(k => k.Item1).ToArray();
            var relationTypes = keys.All(k => k.Item2 != null)
                ? keys.SelectMany(k => k.Item2).GroupBy(r => r.Id).Select(g => g.First()).ToArray()
                : null;
            var nodes = await _ontologyService.LoadNodesAsync(nodeIds, relationTypes, cancellationToken);
            var nodesDict = nodes.ToDictionary(n => n.Id);
            return nodeIds.Select(id => (Result<Node>)nodesDict.GetOrDefault(id)).ToList();
        }
    }
}
