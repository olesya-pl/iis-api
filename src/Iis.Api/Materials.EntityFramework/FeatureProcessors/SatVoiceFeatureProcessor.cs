using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials.FeatureProcessors;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class SatVoiceFeatureProcessor : BasePhoneSignFeatureProcessor, IFeatureProcessor
    {
        protected override string SignTypeName => "SatellitePhoneSign";

        protected override List<string> PrioritizedFields => new List<string>
        {
            FeatureFields.PhoneNumber,
            FeatureFields.Object,
            FeatureFields.IPv4
        };

        protected override Dictionary<string, string> SignFieldsMapping => new Dictionary<string, string>
        {
            { SignFields.PhoneNumber, FeatureFields.PhoneNumber },
            { SignFields.Object, FeatureFields.Object },
            { SignFields.IPv4, FeatureFields.IPv4 },
            { SignFields.Beam, FeatureFields.Beam },
            { SignFields.LocationX, FeatureFields.LocationX },
            { SignFields.LocationY, FeatureFields.LocationY}
        };

        public SatVoiceFeatureProcessor(IElasticService elasticService,
            IOntologyModel ontology,
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState)
        : base(elasticService, ontology, createResolver, updateResolver, elasticState)
        { }
    }
}