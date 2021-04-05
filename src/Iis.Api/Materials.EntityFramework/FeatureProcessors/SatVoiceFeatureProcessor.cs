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
    public class SatVoiceFeatureProcessor : BasePhoneSignFeatureProcessor, IFeatureProcessor
    {
        protected override string SignTypeName => "SatellitePhoneSign";

        protected override IReadOnlyCollection<string> PrioritizedFields => new string[]
        {
            SignFields.PhoneNumber,
            SignFields.DBObject,
            SignFields.IPv4
        };

        protected override Dictionary<string, string> SignFieldsMapping => new Dictionary<string, string>
        {
            { FeatureFields.PhoneNumber, SignFields.PhoneNumber},
            { FeatureFields.Object, SignFields.DBObject },
            { FeatureFields.IPv4, SignFields.IPv4 },
            { FeatureFields.Beam, SignFields.Beam },
            { FeatureFields.LocationX, SignFields.LocationX },
            { FeatureFields.LocationY, SignFields.LocationY }
        };

        public SatVoiceFeatureProcessor(IElasticService elasticService,
            IOntologySchema ontologySchema, 
            MutationCreateResolver createResolver,
            MutationUpdateResolver updateResolver,
            IElasticState elasticState)
        : base(elasticService, ontologySchema, createResolver, updateResolver, elasticState)
        {}
    }
}