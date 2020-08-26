using System;
using System.Threading.Tasks;
using Iis.DataModel.Annotations;

namespace Iis.DbLayer.Repositories
{
    public interface IAnnotationsRepository
    {
        Task<AnnotationEntity> GetByIdAsync(Guid id);
        Task AddAsync(AnnotationEntity entity);
        Task UpdateAsync(AnnotationEntity entity);
    }
}