using System;
using IIS.Core;
using Microsoft.Extensions.Configuration;
using Iis.DataModel;
using HotChocolate;
using System.Threading.Tasks;

namespace Iis.Api
{
    public class MLendpoint
    {      
        public static async void CreateResponse([Service] OntologyContext context, Guid MaterialId, String MLHandlerName, String OriginalResponse)
        {
            await context.Semaphore.WaitAsync();

            var MLResponse = new Iis.DataModel.MLResponseEntity
            {
                Id = Guid.NewGuid(),
                MaterialId = MaterialId,
                MLHandlerName = MLHandlerName,
                OriginalResponse = OriginalResponse
            };
            context.MLResponses.Add(MLResponse);
            context.SaveChanges();
        }
    }
}
