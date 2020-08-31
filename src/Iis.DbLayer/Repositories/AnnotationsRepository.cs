using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.DataModel.Annotations;
using IIS.Repository;


namespace Iis.DbLayer.Repositories
{
    internal class AnnotationsRepository : RepositoryBase<OntologyContext>, IAnnotationsRepository
    {
        public Task<AnnotationEntity> GetByIdAsync(Guid id)
        {
            return Context.Annotations
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == id);
        }

        public void Add(AnnotationEntity entity)
        {
            Context.Annotations.Add(entity);
        }

        public void Update(AnnotationEntity entity)
        {
            Context.Annotations.Update(entity);
        }
    }
}