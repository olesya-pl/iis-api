using System;
using System.Threading.Tasks;
using HotChocolate;
using Newtonsoft.Json.Linq;
using Iis.Services;
using Contracts = Iis.Services.Contracts.Annotations;
namespace IIS.Core.GraphQL.Annotations
{
    public class Mutation
    {
        public Task<Annotation> SetAnnotation(
            [Service] IAnnotationsService annotationsService,
            [GraphQLNonNullType] AnnotationInput input
        )
        {
            var s = annotationsService.SetAnnotationAsync(new Contracts.Annotation()).GetAwaiter().GetResult();
            var result = new Annotation
            {
                Id = input.Id,
                Content = JObject.Parse(input.Content)
            };
            
            return Task.FromResult(result);
        }
    }
}