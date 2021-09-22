using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Materials.FeatureProcessors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    public class DummyFeatureProcessor : IFeatureProcessor
    {
        public bool IsDummy => true;

        public IEnumerable<Guid> GetValidFeatureIds(IEnumerable<Guid> featureIdList)
        {
            return featureIdList;
        }

        public Task<JObject> ProcessMetadataAsync(ProcessingMaterialEntry entry)
        {
            return Task.FromResult(entry.Metadata);
        }
    }

}