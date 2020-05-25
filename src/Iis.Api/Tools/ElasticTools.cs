using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IIS.Core.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.Tools
{
    public class ElasticTools
    {
        IExtNodeService _extNodeService;
        IElasticManager _elasticManager;
        IElasticService _elasticService;
        IMaterialProvider _materialProvider;
        IOntologySchema _ontologySchema;

        public ElasticTools(IExtNodeService extNodeService, IMaterialProvider materialProvider, IElasticService elasticService, IElasticManager elasticManager,
            IOntologySchema ontologySchema)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
            _elasticService = elasticService;
            _materialProvider = materialProvider;
            _ontologySchema = ontologySchema;
        }

        public async Task RecreateElasticAsync(CancellationToken cancellationToken = default)
        {
            var ontologyIndexes = _elasticService.OntologyIndexes;
            
             await _elasticManager.DeleteIndexesAsync(ontologyIndexes, cancellationToken);

            foreach (var ontologyIndex in ontologyIndexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(ontologyIndex);
                var attributesInfo = type.GetAttributesInfo();
                await _elasticManager.CreateMapping(attributesInfo);
            }

            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(ontologyIndexes, cancellationToken);
            foreach (var extNode in extNodes)
            {
                await _elasticService.PutNodeAsync(extNode, cancellationToken);
            }

            await _elasticManager.CreateIndexesAsync(ontologyIndexes, cancellationToken);
            
            var materialEntities = await _materialProvider.GetMaterialEntitiesAsync();

            var materialIndex = _elasticService.MaterialIndexes.First();

            await _elasticManager.DeleteIndexAsync(materialIndex, cancellationToken);

            var entityTasks = materialEntities
                                .Select(async entity => {
                                    var document = await _materialProvider.GetMaterialDocumentAsync(entity.Id);
                                    return _elasticService.PutMaterialAsync(entity.Id, document, cancellationToken);
                                });

            await Task.WhenAll(entityTasks);
        }
    }
}
