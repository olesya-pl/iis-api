using Iis.Interfaces.Elastic;
using IIS.Domain;
using IIS.Core.Materials.FeatureProcessors;
using IIS.Core.GraphQL.Entities.Resolvers;
using Iis.Domain;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class FeatureProcessorFactory : IFeatureProcessorFactory
    {
        private readonly IElasticService _elasticService;
        private readonly IOntologyModel _ontology;
        private readonly MutationCreateResolver _createResolver;
        private readonly MutationUpdateResolver _updateResolver;
        public FeatureProcessorFactory(IElasticService elasticService, IOntologyModel ontology, MutationCreateResolver createResolver, MutationUpdateResolver updateResolver)
        {
            _elasticService = elasticService;
            _ontology = ontology;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
        }
        public IFeatureProcessor GetInstance(string materialSource, string materialType)
        {
            return materialSource switch
            {
                "cell.voice" when materialType == "audio" => new GSMFeatureProcessor(_elasticService, _ontology, _createResolver, _updateResolver),
                "sat.voice" when materialType == "audio" => new SatVoiceFeatureProcessor(_elasticService, _ontology, _createResolver, _updateResolver),
                _ => new DummyFeatureProcessor()
            };
        }
    }
}