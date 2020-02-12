using Iis.Api;
using Iis.Domain;
using Iis.Domain.Elastic;
using Iis.Domain.ExtendedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService
    {
        private IElasticManager _elasticManager;
        private IExtNodeService _extNodeService;
        private RunTimeSettings _runTimeSettings;
        public ElasticService(IElasticManager elasticManager, IExtNodeService extNodeService, RunTimeSettings runTimeSettings)
        {
            _elasticManager = elasticManager;
            _extNodeService = extNodeService;
            _runTimeSettings = runTimeSettings;
        }
        public async Task<List<Guid>> SearchByAllFieldsAsync(IEnumerable<NodeType> nodeTypes, string suggestion, CancellationToken cancellationToken = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                ResultFields = nodeTypes.Select(nt => nt.Name).ToList(),
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
    }
}
