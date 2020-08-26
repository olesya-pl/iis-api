using System;
using System.Threading.Tasks;
using HotChocolate;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Annotations
{
    public class Query
    {
        public Task<Annotation> GetAnnotation(
            [GraphQLNonNullType] Guid annotationId
        )
        {
            var result = new Annotation
            {
                Id = annotationId,
                Content = JObject.Parse("{'type':'text', 'text': 'awesome json'}")
            };
            return Task.FromResult(result);
        }
    }
}