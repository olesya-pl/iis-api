using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using IIS.Core.Ontology;
using Iis.Domain;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Elastic;
using Iis.Domain.Elastic;

namespace IIS.Core.GraphQL.DataLoaders
{
    public class NodeDataLoader : DataLoaderBase<Tuple<Guid, IEmbeddingRelationTypeModel>, Node>
    {
        private readonly IOntologyService _ontologyService;

        public NodeDataLoader(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }

        protected override async Task<IReadOnlyList<Result<Node>>> FetchAsync(IReadOnlyList<Tuple<Guid, IEmbeddingRelationTypeModel>> keys, CancellationToken cancellationToken)
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

    public class QueryNodeDataLoader : DataLoaderBase<Tuple<Guid, IEmbeddingRelationTypeModel>, JObject>
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticService _elasticService;

        public QueryNodeDataLoader(IElasticService elasticService,
            IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
            _elasticService = elasticService;
        }

        protected override async Task<IReadOnlyList<Result<JObject>>> FetchAsync(IReadOnlyList<Tuple<Guid, IEmbeddingRelationTypeModel>> keys, CancellationToken cancellationToken)
        {
            return (await _elasticManager
                .GetDocumentByIdAsync(_elasticService.OntologyIndexes.ToArray(), keys.First().Item1.ToString("N")))
                .Items
                .Select(p => (Result<JObject>)p.SearchResult)
                .ToList();
        }
    }
}
