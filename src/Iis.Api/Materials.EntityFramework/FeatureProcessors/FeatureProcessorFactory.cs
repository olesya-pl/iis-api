using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class FeatureProcessorFactory : IFeatureProcessorFactory
    {
        private readonly IElasticService _elasticService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IElasticState _elasticState;
        private readonly MutationCreateResolver _createResolver;
        private readonly MutationUpdateResolver _updateResolver;
        private readonly IGsmLocationService _gsmLocationService;
        public FeatureProcessorFactory(IElasticService elasticService, IOntologySchema ontologySchema, MutationCreateResolver createResolver, MutationUpdateResolver updateResolver, IElasticState elasticState, IGsmLocationService gsmLocationService)
        {
            _elasticService = elasticService;
            _ontologySchema = ontologySchema;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
            _elasticState = elasticState;
            _gsmLocationService = gsmLocationService;
        }
        public IFeatureProcessor GetInstance(string materialSource, string materialType)
        {
            return materialSource switch
            {
                "cell.voice" when materialType == "audio" => new CellVoiceFeatureProcessor(_elasticService, _ontologySchema, _createResolver, _updateResolver, _elasticState, _gsmLocationService),
                "sat.voice" when materialType == "audio" => new SatVoiceFeatureProcessor(_elasticService, _ontologySchema, _createResolver, _updateResolver, _elasticState),
                "sat.iridium.voice" when materialType == "audio" => new SatVoiceIridiumFeatureProcessor(_elasticService, _ontologySchema, _createResolver, _updateResolver, _elasticState),
                "sat.iridium.paging" when materialType == "text" => new SatPagingIridiumFeatureProcessor(_elasticService, _ontologySchema, _createResolver, _updateResolver, _elasticState),
                _ => new DummyFeatureProcessor()
            };
        }
    }
}