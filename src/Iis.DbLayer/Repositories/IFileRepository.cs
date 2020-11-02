using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using System.Linq.Expressions;

namespace Iis.DbLayer.Repositories
{
    public interface IFileRepository 
    {
        Task<FileEntity> GetAsync(Expression<Func<FileEntity, bool>> predicate);
        Task<List<FileEntity>> GetManyAsync(Expression<Func<FileEntity, bool>> predicate);
        void Create(FileEntity entity);
        void Update(FileEntity entity);
        void RemoveRange(IEnumerable<FileEntity> files);
    }
}