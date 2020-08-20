using System;
using System.Threading.Tasks;

using Iis.DataModel;
using Iis.DataModel.Annotations;
using IIS.Repository;

namespace Iis.DbLayer.Repositories
{
    internal class AnnotationsRepository : RepositoryBase<OntologyContext>, IAnnotationsRepository
    {
        public Task<AnnotationEntity> GetByIdAsync(Guid id)
        {
            var result = new AnnotationEntity
            {
                Id = Guid.NewGuid(),
                Content = "{'type':'text'}"
            };

            return Task.FromResult(result);
        }

        public Task<AnnotationEntity> SaveAsync(AnnotationEntity entity)
        {
            var result = new AnnotationEntity
            {
                Id = Guid.NewGuid(),
                Content = "{'type':'text'}"
            };
            
            return Task.FromResult(result);
        }
    }
}