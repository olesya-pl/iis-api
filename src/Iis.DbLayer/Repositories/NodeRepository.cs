using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly IElasticManager _elasticManager;
        private readonly NodeFlattener<IIISUnitOfWork> _nodeFlattener;
        private readonly IChangeHistoryService _changeHistoryService;

        public NodeRepository(IElasticManager elasticManager,
            NodeFlattener<IIISUnitOfWork> nodeFlattener,
            IOntologySchema ontologySchema, IChangeHistoryService changeHistoryService)
        {
            _elasticManager = elasticManager;
            _nodeFlattener = nodeFlattener;
            _changeHistoryService = changeHistoryService;

            var objectOfStudyType = ontologySchema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            if (objectOfStudyType != null)
            {
                OntologyIndexes = objectOfStudyType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToArray();
            }
        }

        public IReadOnlyCollection<string> OntologyIndexes { get; }

        public async Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _elasticManager.GetDocumentByIdAsync(OntologyIndexes, id.ToString("N"), cancellationToken);
            return result.Items.FirstOrDefault()?.SearchResult;
        }

        public async Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _nodeFlattener.FlattenNode(id, cancellationToken);
            return await _elasticManager.PutDocumentAsync(
                result.NodeTypeName,
                result.Id,
                result.SerializedNode, cancellationToken);
        }

        public async Task<bool> PutHistoricalNodesAsync(Guid id, CancellationToken ct = default) 
        {
            var getNodeChanges = _changeHistoryService.GetChangeHistory(id, null);
            var getActualNode = _nodeFlattener.FlattenNode(id, ct);

            await Task.WhenAll(getActualNode, getNodeChanges);

            
            var changes = getNodeChanges.Result
                .GroupBy(x => x.RequestId)
                .Where(x => x.Count() > 0)
                .OrderByDescending(x => x.First().Date)
                .ToList();

            var actualNode = getActualNode.Result;
            var nodes = new List<FlattenNodeResult>();
            foreach (var changePack in changes)
            {
                var olderNode = new FlattenNodeResult
                {
                    Id = actualNode.Id,
                    NodeTypeName = actualNode.NodeTypeName,
                    SerializedNode = actualNode.SerializedNode.ReplaceOrAddValues(changePack.Select(x => (x.PropertyName, x.OldValue)).ToArray())
                };
                nodes.Add(olderNode);
                actualNode = olderNode;
            }

            foreach (var item in nodes)
            {
                var result = await _elasticManager.PutDocumentAsync(
                $"historical_{item.NodeTypeName}",
                Guid.NewGuid().ToString(),
                item.SerializedNode, ct);
            }

            return true;

            //return await _elasticManager.PutsDocumentsAsync($"historical_{actualNode.NodeTypeName}", string.Join("", nodes.Select(x => x.SerializedNode)), ct);
        }
    }
}
