using System;
using System.Threading.Tasks;
using HotChocolate;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Annotations
{
    public class Mutation
    {
        public Task<Annotation> SetAnnotation(
            [GraphQLNonNullType] AnnotationInput input
        )
        {
            var result = new Annotation
            {
                Id = input.Id,
                Content = JObject.Parse(input.Content)
            };
            
            return Task.FromResult(result);
        }
    }
}