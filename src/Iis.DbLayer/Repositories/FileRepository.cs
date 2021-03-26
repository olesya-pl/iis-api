using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using System.Linq.Expressions;
using IIS.Repository;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Iis.DbLayer.Repositories
{
    public class FileRepository : RepositoryBase<OntologyContext>, IFileRepository
    {
        public void Create(FileEntity entity)
        {
            Context.Files.Add(entity);
        }

        public Task<FileEntity> GetAsync(Expression<Func<FileEntity, bool>> predicate)
        {
            return Context.Files.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public Task<List<FileEntity>> GetManyAsync(Expression<Func<FileEntity, bool>> predicate)
        {
            return Context.Files.AsNoTracking().Where(predicate).ToListAsync();
        }

        public void RemoveRange(IEnumerable<FileEntity> files)
        {
            Context.Files.RemoveRange(files);
        }

        public Task<List<Guid>> GetMaterialFileIds()
        {
            return Context.Materials
                .Where(x => x.FileId.HasValue)
                .Select(x => x.FileId.Value)
                .ToListAsync();
        }

        public void Update(FileEntity entity)
        {
            Context.Files.Update(entity);
        }
    }
}