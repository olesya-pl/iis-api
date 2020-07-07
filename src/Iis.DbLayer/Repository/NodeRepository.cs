using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Repository;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repository
{
    public class NodeRepository : INodeRepository
    {
        private readonly IElasticManager _elasticManager;

        public NodeRepository(IElasticManager elasticManager,
            IOntologySchema ontologySchema)
        {
            _elasticManager = elasticManager;

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

        public async Task<JObject> GetNodeById(Guid id)
        {
            var result = await _elasticManager.GetDocumentByIdAsync(OntologyIndexes, id.ToString("N"));
            return result.Items.FirstOrDefault()?.SearchResult;
        }
    }
}
