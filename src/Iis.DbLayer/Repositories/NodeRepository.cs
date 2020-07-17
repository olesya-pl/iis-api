using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    internal class NodeRepository : INodeRepository
    {
        private readonly IElasticManager _elasticManager;
        private readonly NodeFlattener _nodeFlattener;

        public NodeRepository(IElasticManager elasticManager,
            NodeFlattener nodeFlattener,
            IOntologySchema ontologySchema)
        {
            _elasticManager = elasticManager;
            _nodeFlattener = nodeFlattener;

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
    }
}
