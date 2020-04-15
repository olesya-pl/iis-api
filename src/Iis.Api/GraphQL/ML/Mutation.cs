using System;
using Iis.DataModel;
using HotChocolate;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.ML
{
    public class Mutation
    {      
        public async Task<MachineLearningResult> CreateMachineLearningResult(
            [Service] OntologyContext context, 
            [GraphQLNonNullType] MachineLearningResponseInput input)
        {
            await context.Semaphore.WaitAsync();
            try
            {
                var mlResponse = new Iis.DataModel.MLResponseEntity
                {
                    Id = Guid.NewGuid(),
                    MaterialId = input.MaterialId,
                    MLHandlerName = input.HandlerName,
                    OriginalResponse = input.OriginalResponse
                };
                
                context.MLResponses.Add(mlResponse);
                
                context.SaveChanges();
                
                return new MachineLearningResult
                {
                    Id = mlResponse.Id,
                    MaterialId = mlResponse.MaterialId,
                    HandlerName = mlResponse.MLHandlerName
                };
            }
            finally 
            { 
                context.Semaphore.Release(); 
            }
        }
    }
}
