using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Elastic;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    internal class ElasticFieldsRepository : RepositoryBase<OntologyContext>, IElasticFieldsRepository
    {
        public async Task AddField(List<ElasticFieldEntity> entitiesToAdd, List<ElasticFieldEntity> entititesToUpdate)
        {
            await Context.AddRangeAsync(entitiesToAdd);
            Context.UpdateRange(entititesToUpdate);
        }

        public Task<Dictionary<string, ElasticFieldEntity>> GetElasticFieldsByTypename(string typeName)
        {
            return Context.ElasticFields.Where(ef => ef.TypeName == typeName).ToDictionaryAsync(ef => ef.Name, ef => ef);
        }

        public Task<List<ElasticFieldEntity>> GetReadonlyElasticFieldsByTypename(string typeName)
        {
            return Context.ElasticFields.Where(ef => ef.TypeName == typeName).AsNoTracking().ToListAsync();
        }        
    }
}