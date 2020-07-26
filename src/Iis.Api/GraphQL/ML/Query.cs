using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;

namespace IIS.Core.GraphQL.ML
{
    public class Query
    {
        public async Task<List<MachineLearningResult>> GetMlProcessingResults(
            [Service] IMaterialProvider materialProvider, 
            [Service] IMapper mapper,
            Guid materialId)
        {
            var mlResults = await materialProvider.GetMLProcessingResultsAsync(materialId);
            
            return mlResults.Select(p => mapper.Map<MachineLearningResult>(p)).ToList();
        } 
    }
}
