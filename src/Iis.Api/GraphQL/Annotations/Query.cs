using System;
using System.Threading.Tasks;
using HotChocolate;
using AutoMapper;
using Newtonsoft.Json.Linq;

using Iis.Services;
namespace IIS.Core.GraphQL.Annotations
{
    public class Query
    {
        public async Task<Annotation> GetAnnotation(
            [Service] IAnnotationsService annotationsService,
            [Service] IMapper _mapper,
            [GraphQLNonNullType] Guid annotationId
        )
        {
            var annotation = await annotationsService.GetAnnotationAsync(annotationId);
            
            return _mapper.Map<Annotation>(annotation);
        }
    }
}