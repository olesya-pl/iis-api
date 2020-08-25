using System;
using System.Threading.Tasks;
using Iis.Services.Contracts.Annotations;

namespace Iis.Services
{
    public interface IAnnotationsService
    {
        Task<Annotation> SetAnnotationAsync(Annotation annotation);
        Task<Annotation> GetAnnotationAsync(Guid annotationId);
    }
}