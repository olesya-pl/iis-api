using System;
using AutoMapper;
using HotChocolate;
using System.Threading.Tasks;

using IIS.Core.ML;
using Iis.DataModel;
using Iis.Domain.MachineLearning;

namespace IIS.Core.GraphQL.ML
{
    public class Mutation
    {      
        public async Task<MachineLearningResult> CreateMachineLearningResult(
            [Service] MlProcessingService service,
            [Service] IMapper mapper, 
            [GraphQLNonNullType] MachineLearningResponseInput input)
        {
            var response = mapper.Map<MlResponse>(input);

            var result = await service.AddMlHandlerResponseAsync(response);

            return mapper.Map<MachineLearningResult>(result);
        }
    }
}
