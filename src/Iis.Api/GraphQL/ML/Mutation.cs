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
            var response = mapper.Map<MLResponse>(input);

            var result = await service.SaveMlHandlerResponseAsync(response);

            return mapper.Map<MachineLearningResult>(result);
        }

        public async Task<MachineLearningHadnlersCountResult> SetMachineLearningHadnlersCount(
            [Service] IMaterialService service,
            [Service] IMapper mapper,
            [GraphQLNonNullType] MachineLearningHadnlersCountInput input)
        {
            await service.SetMachineLearningHadnlersCount(input.MaterialId, input.HandlersCount);
            
            return mapper.Map<MachineLearningHadnlersCountResult>(input);

        }
    }
}
