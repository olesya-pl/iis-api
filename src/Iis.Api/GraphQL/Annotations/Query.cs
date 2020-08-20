using System;
using System.Threading.Tasks;
using HotChocolate;
using Newtonsoft.Json.Linq;
using IIS.Core.Materials;

namespace IIS.Core.GraphQL.Annotations
{
    public class Query
    {
        public async Task<Annotation> GetAnnotation(
            [Service] IMaterialProvider materialProvider,
            [GraphQLNonNullType] Guid annotationId
        )
        {
            var result = new Annotation
            {
                Id = annotationId,
                Content = JObject.Parse("{'type':'text', 'text': 'awesome json'}")
            };
            return await Task.FromResult(result);
        }
    }
}