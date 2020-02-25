using Iis.Api;
using Iis.Domain;
using Iis.Domain.Elastic;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private IElasticManager _elasticManager;
        private IExtNodeService _extNodeService;
        private RunTimeSettings _runTimeSettings;
        private readonly OntologyContext _context;

        public ElasticService(IElasticManager elasticManager, IExtNodeService extNodeService, RunTimeSettings runTimeSettings, OntologyContext context)
        {
            _elasticManager = elasticManager;
            _extNodeService = extNodeService;
            _runTimeSettings = runTimeSettings;
            _context = context;
        }
        public async Task<List<Guid>> SearchByAllFieldsAsync(IEnumerable<string> typeNames, string suggestion, CancellationToken cancellationToken = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = $"*{suggestion}*"
            };
            var ids = await _elasticManager.Search(searchParams, cancellationToken);
            return ids.Select(id => new Guid(id)).ToList();
        }

        public async Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_runTimeSettings.PutSavedToElastic) return true;
            var extNode = await _extNodeService.GetExtNodeByIdAsync(id, cancellationToken);
            return await _elasticManager.PutExtNodeAsync(extNode, cancellationToken);
        }

        public bool TypesAreSupported(IEnumerable<string> typeNames)
        {
            return _elasticManager.IndexesAreSupported(typeNames);
        }

        public async Task UpdateElasticAsync(Guid nodeTypeId, string indexName, CancellationToken cancellationToken = default)
        {
            ElasticCompareResult compareResult = await CompareWithElasticAsync(nodeTypeId, indexName, cancellationToken);

            foreach (Guid id in compareResult.NeedToDelete)
            {
                await _elasticManager.DeleteAsync(indexName, id.ToString("N"));
            }

            foreach (Guid id in compareResult.NeedToUpdate)
            {
                await PutNodeAsync(id, cancellationToken);
            }
        }

        public async Task<ElasticCompareResult> CompareWithElasticAsync(Guid nodeTypeId, string indexName, CancellationToken cancellationToken = default)
        {
            List<string> list = await _elasticManager.GetIndexIdsAsync(indexName);
            var elasticIds = list.Select(x => Guid.ParseExact(x, "N"));

            var query =
                from n in _context.Nodes
                where !n.IsArchived && n.NodeTypeId == nodeTypeId
                select n.Id;
            List<Guid> dbIds = await query.ToListAsync(cancellationToken);

            var toDelete = elasticIds.ToHashSet();
            toDelete.ExceptWith(dbIds);

            return new ElasticCompareResult
            {
                NeedToDelete = toDelete.ToList(),
                NeedToUpdate = dbIds
            };
        }

        public Task<string> GetNodeByIdAsync(string indexName, string id, IEnumerable<NodeType> nodeTypes)
        {
            return _elasticManager.GetByIdAsync(indexName, id, nodeTypes.Select(nt => nt.Name).ToArray());
        }
    }
}
