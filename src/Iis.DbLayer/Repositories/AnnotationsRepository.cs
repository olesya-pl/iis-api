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
                Content = "{'type':'text', 'text': 'awesome json'}"
            };

            return Task.FromResult(result);
        }

        public Task AddAsync(AnnotationEntity entity)
        {
            var result = new AnnotationEntity
            {
                Id = entity.Id,
                Content = entity.Content
            };
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AnnotationEntity entity)
        {
            var result = new AnnotationEntity
            {
                Id = entity.Id,
                Content = entity.Content
            };
            return Task.CompletedTask;
        }
    }
}