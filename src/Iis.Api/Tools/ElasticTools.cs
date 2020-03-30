using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IIS.Core.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;


namespace IIS.Core.Tools
{
    public class ElasticTools
    {
        IExtNodeService _extNodeService;
        IElasticManager _elasticManager;
        IElasticService _elasticService;
        IMaterialProvider _materialProvider;

        public ElasticTools(IExtNodeService extNodeService, IMaterialProvider materialProvider, IElasticService elasticService, IElasticManager elasticManager)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
            _elasticService = elasticService;
            _materialProvider = materialProvider;
        }

        public async Task RecreateElasticAsync(CancellationToken cancellationToken = default)
        {
            var ontologyIndexes = _elasticService.OntologyIndexes;

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, cancellationToken);
            
            await _elasticManager.DeleteIndexesAsync(ontologyIndexes, cancellationToken);
            
            foreach (var extNode in extNodes)
            {
                await _elasticService.PutNodeAsync(extNode, cancellationToken);
            }

            await _elasticManager.CreateIndexesAsync(ontologyIndexes, cancellationToken);


            var materialEntities = await _materialProvider.GetMaterialEntitiesAsync();

            var materialIndex = _elasticService.MaterialIndexes.First();

            await _elasticManager.DeleteIndexAsync(materialIndex, cancellationToken);

            var entityTasks = materialEntities
                                .Select(async entity => await _elasticService.PutMaterialAsync(entity, cancellationToken));

            await Task.WhenAll(entityTasks);
        }
    }
}
