using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;
using Iis.Interfaces.Constants;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Ontology.Schema;

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

        protected override Task PostMetadataProcessingAsync(JObject metadata, Guid materialId)
        {
            return _gsmLocationService.TryFillTowerLocationHistory(metadata, materialId);
        }

        public CellVoiceFeatureProcessor(IElasticService elasticService,
            IOntologySchema ontologySchema,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState,
            IGsmLocationService gsmLocationService,
            ILocationHistoryService locationHistoryService,
            ILogger logger)
        : base(elasticService, ontologySchema, createResolver, updateResolver, elasticState, locationHistoryService, logger)
        {
            _gsmLocationService = gsmLocationService;
        }

        protected override Task SaveCoordinatesToLocationHistoryAsync(JObject feature, DateTime locationTimeStamp)
        {
            return Task.CompletedTask;
        }
    }

}