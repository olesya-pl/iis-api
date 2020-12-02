using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel;
using Iis.Interfaces.Enums;

namespace Iis.DbLayer.Repositories
{
    public interface IAliasRepository 
    {
        Task<List<AliasEntity>> GetByTypeAsync(AliasType type);
     
        Task<List<AliasEntity>> GetAllAsync();
        
        void Create(AliasEntity entity);

        void CreateRange(List<AliasEntity> entities);

        void Update(AliasEntity entity);

        void Remove(AliasEntity entity);

        Task<AliasEntity> GetByIdAsync(Guid id);
    }
}