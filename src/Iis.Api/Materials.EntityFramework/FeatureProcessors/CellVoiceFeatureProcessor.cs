using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class CellVoiceFeatureProcessor : BasePhoneSignFeatureProcessor, IFeatureProcessor
    {
        private readonly IGsmLocationService _gsmLocationService;
        protected override string SignTypeName => "CellphoneSign";

        protected override IReadOnlyCollection<string> PrioritizedFields => new string[]
        {
            SignFields.IMSI,
            SignFields.PhoneNumber,
            SignFields.IMEI,
            SignFields.DBObject
        };

        protected override Dictionary<string, string> SignFieldsMapping => new Dictionary<string, string>
        {
            { FeatureFields.PhoneNumber, SignFields.PhoneNumber },
            { FeatureFields.DBObject, SignFields.DBObject },
            { FeatureFields.IMEI, SignFields.IMEI },
            { FeatureFields.IMSI, SignFields.IMSI },
            { FeatureFields.TMSI, SignFields.TMSI }
        };

        public CellVoiceFeatureProcessor(IElasticService elasticService,
            IOntologySchema ontologySchema,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState,
            IGsmLocationService gsmLocationService,
            ILocationHistoryService locationHistoryService)
        : base(elasticService, ontologySchema, createResolver, updateResolver, elasticState, locationHistoryService)
        {
            _gsmLocationService = gsmLocationService;
        }

        protected override Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoryCollectionAsync(ProcessingMaterialEntry entry)
        {
            return _gsmLocationService.GetLocationHistoryCollectionAsync(entry.Metadata, entry.Id);
        }
    }

}