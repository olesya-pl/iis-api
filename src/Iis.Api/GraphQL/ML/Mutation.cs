using System;
using Iis.DataModel;
using HotChocolate;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.ML
{
    public class Mutation
    {      
        public static async Task CreateResponse([Service] OntologyContext context, MlProcessingResponseDto data)
        {
            await context.Semaphore.WaitAsync();
            try
            {
                var MLResponse = new Iis.DataModel.MLResponseEntity
                {
                    Id = Guid.NewGuid(),
                    MaterialId = data.MaterialId,
                    MLHandlerName = data.MLHandlerName,
                    OriginalResponse = data.OriginalResponse
                };
                context.MLResponses.Add(MLResponse);
                context.SaveChanges();
            }
            finally { context.Semaphore.Release(); }
        }
    }
}
