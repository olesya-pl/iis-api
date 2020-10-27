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
    public interface IFileRepository 
    {
        Task<FileEntity> GetAsync(Expression<Func<FileEntity, bool>> predicate, bool trackChanges = false);
        Task<List<FileEntity>> GetManyAsync(Expression<Func<FileEntity, bool>> predicate, bool trackChanges = false);
        void Create(FileEntity entity);
        void Update(FileEntity entity);
        void RemoveRange(IEnumerable<FileEntity> files);
    }

    public class FileRepository : RepositoryBase<OntologyContext>, IFileRepository
    {
        public void Create(FileEntity entity)
        {
            Context.Files.Add(entity);
        }

        public Task<FileEntity> GetAsync(Expression<Func<FileEntity, bool>> predicate, bool trackChanges = false)
        {
            return trackChanges 
                ? Context.Files.FirstOrDefaultAsync(predicate) 
                : Context.Files.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public Task<List<FileEntity>> GetManyAsync(Expression<Func<FileEntity, bool>> predicate, bool trackChanges = false)
        {
            return trackChanges
               ? Context.Files.Where(predicate).ToListAsync()
               : Context.Files.AsNoTracking().Where(predicate).ToListAsync();
        }

        public void RemoveRange(IEnumerable<FileEntity> files)
        {
            Context.Files.RemoveRange(files);
        }

        public void Update(FileEntity entity)
        {
            Context.Files.Update(entity);
        }
    }
}