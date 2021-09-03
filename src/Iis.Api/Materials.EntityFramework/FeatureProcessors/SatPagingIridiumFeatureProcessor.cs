using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class SatPagingIridiumFeatureProcessor : BasePhoneSignFeatureProcessor, IFeatureProcessor
    {
        protected override string SignTypeName => "SatelliteIridiumPhoneSign";

        public SatPagingIridiumFeatureProcessor(IElasticService elasticService,
            IOntologySchema ontologySchema,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState,
            ILocationHistoryService locationHistoryService,
            ILogger logger)
        : base(elasticService, ontologySchema, createResolver, updateResolver, elasticState, locationHistoryService, logger)
        {
        }


        protected override IReadOnlyCollection<string> PrioritizedFields => new string[]
        {
            SignFields.IMSI,
            SignFields.PhoneNumber,
            SignFields.IMEI,
        };

        protected override Dictionary<string, string> SignFieldsMapping => new Dictionary<string, string>
        {
            { FeatureFields.PagerId, SignFields.PhoneNumber },
            { FeatureFields.IMEI, SignFields.IMEI },
            { FeatureFields.IMSI, SignFields.IMSI },
            { FeatureFields.TMSI, SignFields.TMSI },
            { FeatureFields.Beam, SignFields.Beam }
        };
    }
}