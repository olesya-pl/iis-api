using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HotChocolate;
using IIS.Core.Materials;

namespace IIS.Core.GraphQL.ML
{
    public class Query
    {
        public List<MlProcessingResult> GetMlProcessingResults([Service] IMaterialProvider materialProvider, 
            [Service] IMapper mapper,
            Guid materialId)
        {
            var mlResults = materialProvider.GetMlProcessingResults(materialId);
            
            return mlResults.Select(p => mapper.Map<MlProcessingResult>(p)).ToList();
        } 
    }
}
