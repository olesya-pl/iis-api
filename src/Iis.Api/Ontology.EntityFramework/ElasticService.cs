using Iis.Domain;
using Iis.Domain.Elastic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService
    {
        private IElasticManager _elasticManager;
        public ElasticService(IElasticManager elasticManager)
        {
            _elasticManager = elasticManager;
        }
        public async Task<List<Guid>> SearchByAllFields(IEnumerable<NodeType> nodeTypes, string suggestion)
        {
            var searchParams = new IisElasticSearchParams
            {
                ResultFields = nodeTypes.Select(nt => nt.Name).ToList(),
                Query = $"*{suggestion}*"
            };
            var ids = await _elasticManager.Search(searchParams);
            return ids.Select(id => new Guid(id)).ToList();
        }
    }
}
