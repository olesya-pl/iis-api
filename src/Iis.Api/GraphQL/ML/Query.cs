using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HotChocolate;
using IIS.Core.ML;

namespace IIS.Core.GraphQL.ML
{
    public class Query
    {
        public List<MlProcessingResult> GetMlProcessingResults([Service] MlProcessingService service, 
            [Service] IMapper mapper,
            Guid materialId)
        {
            var mlResults = service.GetMlHandlingResults(materialId);
            return mlResults.Select(p => mapper.Map<MlProcessingResult>(p)).ToList();
        } 
    }
}
