using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    internal class TowerLocationRepository : RepositoryBase<OntologyContext>, ITowerLocationRepository
    {
        public Task<List<TowerLocationEntity>> GetAllAsync()
        {
            return Context.TowerLocations.ToListAsync();
        }

        public async Task<(decimal Lat, decimal Long)> GetByCellGlobalIdentityAsync(string mcc, string mnc, string lac,
            string cellId)
        {
            var location = await Context.TowerLocations
                .Where(x => x.Mcc == mcc && x.Mnc == mnc && x.Lac == lac && x.CellId == cellId)
                .Select(x => new {x.Lat, x.Long})
                .FirstOrDefaultAsync();
            
            return (location.Lat, location.Long);
        }
    }
}