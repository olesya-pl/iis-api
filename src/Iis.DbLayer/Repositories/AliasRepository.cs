using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using IIS.Repository;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Iis.Interfaces.Enums;

namespace Iis.DbLayer.Repositories
{
    public class AliasRepository : RepositoryBase<OntologyContext>, IAliasRepository
    {
        public void Create(AliasEntity entity)
        {
            Context.Aliases.Add(entity);
        }

        public void CreateRange(List<AliasEntity> entities)
        {
            Context.Aliases.AddRange(entities);
        }

        public Task<List<AliasEntity>> GetByTypeAsync(AliasType type)
        {
            return Context.Aliases.Where(x => x.Type == type).ToListAsync();
        }

        public Task<AliasEntity> GetByIdAsync(Guid id)
        {
            return Context.Aliases.SingleOrDefaultAsync(x => x.Id == id);
        }

        public void Update(AliasEntity entity)
        {
            Context.Aliases.Update(entity);
        }

        public void Remove(AliasEntity entity)
        {
            Context.Aliases.Remove(entity);
        }

        public Task<List<AliasEntity>> GetAllAsync()
        {
            return Context.Aliases.ToListAsync();
        }
    }
}