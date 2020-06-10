using Iis.Interfaces.Elastic;
using IIS.Domain;
using IIS.Core.Materials.FeatureProcessors;
using IIS.Core.GraphQL.Entities.Resolvers;
namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class FeatureProcessorFactory : IFeatureProcessorFactory
    {
        private readonly IElasticService _elasticService;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly MutationCreateResolver _createResolver;
        private readonly MutationUpdateResolver _updateResolver;
        public FeatureProcessorFactory(IElasticService elasticService, IOntologyProvider ontologyProvider, MutationCreateResolver createResolver, MutationUpdateResolver updateResolver)
        {
            _elasticService = elasticService;
            _ontologyProvider = ontologyProvider;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
        }
        public IFeatureProcessor GetInstance(string materialSource)
        {
            return materialSource switch
            {
                "cell.voice" => new GSMFeatureProcessor(_elasticService, _ontologyProvider, _createResolver, _updateResolver),
                _ => new DummyFeatureProcessor()
            };
        }
    }
}