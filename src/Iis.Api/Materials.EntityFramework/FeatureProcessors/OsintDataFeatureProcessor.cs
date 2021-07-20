using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Data;
using IIS.Core.Materials.FeatureProcessors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework.FeatureProcessors
{
    internal class OsintDataFeatureProcessor : IFeatureProcessor
    {
        private readonly IOntologyNodesData _nodesData;

        public OsintDataFeatureProcessor(IOntologyNodesData nodesData)
        {
            _nodesData = nodesData;
        }

        public bool IsDummy => false;

        public IEnumerable<Guid> GetValidFeatureIds(IEnumerable<Guid> featureIdList)
        {
            return featureIdList.Where(p => _nodesData.GetNode(p) != null);
        }

        public async Task<JObject> ProcessMetadataAsync(JObject metadata, Guid materialId)
        {
            return metadata;           
        }
    }
}