using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class SatVoiceFeatureProcessor : IFeatureProcessor
    {
        public bool IsDummy => false;
        public SatVoiceFeatureProcessor(IElasticService elasticService,
            IOntologyModel ontology,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState)
        {

        }
        public Task<JObject> ProcessMetadataAsync(JObject metadata)
        {
            throw new System.NotImplementedException();
        }
    }
}