using System;
using System.Threading.Tasks;
using HotChocolate;
using Newtonsoft.Json.Linq;
using IIS.Core.Materials;
using Iis.Services;
namespace IIS.Core.GraphQL.Annotations
{
    public class Query
    {
        public async Task<Annotation> GetAnnotation(
            [Service] IAnnotationsService annotationsService,
            [GraphQLNonNullType] Guid annotationId
        )
        {
            var s = await annotationsService.GetAnnotationAsync(annotationId);

            var result = new Annotation
            {
                Id = annotationId,
                Content = JObject.Parse("{'type':'text', 'text': 'awesome json'}")
            };
            return await Task.FromResult(result);
        }
    }
}