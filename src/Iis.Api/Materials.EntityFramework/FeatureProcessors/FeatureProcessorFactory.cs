using Microsoft.Extensions.Logging;
using Iis.Domain;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services;
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
        private readonly ILocationHistoryService _locationHistoryService;
        private readonly IOntologyNodesData _nodesData;
        private readonly NodeMaterialRelationService<IIISUnitOfWork> _nodeMaterialRelationService;
        private readonly ILoggerFactory _loggerFactory;

        public FeatureProcessorFactory(IElasticService elasticService,
            IOntologySchema ontologySchema,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState,
            IGsmLocationService gsmLocationService,
            ILocationHistoryService locationHistoryService,
            IOntologyNodesData nodesData,
            ILoggerFactory loggerFactory)
        {
            _elasticService = elasticService;
            _ontologySchema = ontologySchema;
            _createResolver = createResolver;
            _updateResolver = updateResolver;
            _elasticState = elasticState;
            _gsmLocationService = gsmLocationService;
            _locationHistoryService = locationHistoryService;
            _nodesData = nodesData;
            _loggerFactory = loggerFactory;
        }
        public IFeatureProcessor GetInstance(string materialSource, string materialType)
        {
            return materialSource switch
            {
                "cell.voice" when materialType == "audio" => new CellVoiceFeatureProcessor(
                    _elasticService,
                    _ontologySchema,
                    _createResolver,
                    _updateResolver,
                    _elasticState,
                    _gsmLocationService,
                    _locationHistoryService,
                    _loggerFactory.CreateLogger<CellVoiceFeatureProcessor>()),
                "sat.voice" when materialType == "audio" => new SatVoiceFeatureProcessor(
                    _elasticService,
                    _ontologySchema,
                    _createResolver,
                    _updateResolver,
                    _elasticState,
                    _locationHistoryService,
                    _loggerFactory.CreateLogger<SatVoiceFeatureProcessor>()),
                "sat.iridium.voice" when materialType == "audio" => new SatVoiceIridiumFeatureProcessor(
                    _elasticService,
                    _ontologySchema,
                    _createResolver,
                    _updateResolver,
                    _elasticState,
                    _locationHistoryService,
                    _loggerFactory.CreateLogger<SatVoiceIridiumFeatureProcessor>()),
                "sat.iridium.paging" when materialType == "text" => new SatPagingIridiumFeatureProcessor(
                    _elasticService,
                    _ontologySchema,
                    _createResolver,
                    _updateResolver,
                    _elasticState,
                    _locationHistoryService,
                    _loggerFactory.CreateLogger<SatPagingIridiumFeatureProcessor>()),
                "osint.data.file" => new OsintDataFeatureProcessor(_nodesData),
                _ => new DummyFeatureProcessor()
            };
        }
    }
}