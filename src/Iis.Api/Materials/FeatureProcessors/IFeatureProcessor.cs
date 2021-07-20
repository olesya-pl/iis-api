using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.FeatureProcessors
{
    /// <summary>
    /// Defines FeatureProcessor
    /// </summary>
    public interface IFeatureProcessor
    {
        bool IsDummy { get; }
        Task<JObject> ProcessMetadataAsync(JObject metadata, Guid materialId);
        IEnumerable<Guid> GetValidFeatureIds(IEnumerable<Guid> featureIdList);
    }
}