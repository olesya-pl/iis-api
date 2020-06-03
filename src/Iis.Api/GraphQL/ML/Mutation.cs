using System;
using AutoMapper;
using HotChocolate;
using System.Threading.Tasks;

using IIS.Core.Materials;
using Iis.Domain.MachineLearning;

namespace IIS.Core.GraphQL.ML
{
    public class Mutation
    {      
        public async Task<MachineLearningResult> CreateMachineLearningResult(
            [Service] IMaterialService service,
            [Service] IMapper mapper, 
            [GraphQLNonNullType] MachineLearningResponseInput input)
        {
            var response = mapper.Map<MlResponse>(input);

            var result = await service.SaveMlHandlerResponseAsync(response);

            return mapper.Map<MachineLearningResult>(result);
        }
    }
}
