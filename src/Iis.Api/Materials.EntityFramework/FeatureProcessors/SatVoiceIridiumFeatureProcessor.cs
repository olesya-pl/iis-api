using System.Collections.Generic;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class SatVoiceIridiumFeatureProcessor : BasePhoneSignFeatureProcessor, IFeatureProcessor
    {
        protected override string SignTypeName => "SatelliteIridiumPhoneSign";
        public SatVoiceIridiumFeatureProcessor(IElasticService elasticService,
            IOntologySchema ontologySchema,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState)
        : base(elasticService, ontologySchema, createResolver, updateResolver, elasticState)
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
            { FeatureFields.Latitude, SignFields.LocationY},
            { FeatureFields.Longitude, SignFields.LocationX},
            { FeatureFields.PhoneNumber, SignFields.PhoneNumber },
            { FeatureFields.IMEI, SignFields.IMEI },
            { FeatureFields.IMSI, SignFields.IMSI },
            { FeatureFields.TMSI, SignFields.TMSI },
            { FeatureFields.Beam, SignFields.Beam }
        };
    }
}