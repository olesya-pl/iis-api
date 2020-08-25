using System;
using System.Threading.Tasks;
using Iis.Services.Contracts.Annotations;
namespace Iis.Services
{
    public class AnnotationsService : IAnnotationsService
    {
        public Task<Annotation> GetAnnotationAsync(Guid annotationId)
        {
            throw new NotImplementedException();
        }

        public Task<Annotation> SetAnnotationAsync(Annotation annotation)
        {
            throw new NotImplementedException();
        }
    }
}