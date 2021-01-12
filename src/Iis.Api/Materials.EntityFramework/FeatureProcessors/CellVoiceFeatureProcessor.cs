using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;
using Iis.Interfaces.Constants;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class CellVoiceFeatureProcessor : BasePhoneSignFeatureProcessor, IFeatureProcessor
    {
        private readonly IGsmLocationService _gsmLocationService;
        protected override string SignTypeName => "CellphoneSign";

        protected override List<string> PrioritizedFields => new List<string>
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

        protected override Task PostMetadataProcessingAsync(JObject metadata, Guid materialId)
        {
            return _gsmLocationService.TryFillTowerLocationHistory(metadata, materialId);
        }

        public CellVoiceFeatureProcessor(IElasticService elasticService,
            IOntologyModel ontology,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState, IGsmLocationService gsmLocationService)
        : base(elasticService, ontology, createResolver, updateResolver, elasticState)
        {
            _gsmLocationService = gsmLocationService;
        }
    }

}