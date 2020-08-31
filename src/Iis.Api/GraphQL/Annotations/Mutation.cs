using System;
using System.Threading.Tasks;
using HotChocolate;
using Newtonsoft.Json.Linq;
using AutoMapper;

using Iis.Services;
using Contracts = Iis.Services.Contracts.Annotations;
namespace IIS.Core.GraphQL.Annotations
{
    public class Mutation
    {
        public async Task<Annotation> SetAnnotation(
            [Service] IAnnotationsService _annotationsService,
            [Service] IMapper _mapper,
            [GraphQLNonNullType] AnnotationInput input
        )
        {
            var annotation = _mapper.Map<Contracts.Annotation>(input);

            annotation = await _annotationsService.SetAnnotationAsync(annotation);

            return _mapper.Map<Annotation>(annotation);
        }
    }
}