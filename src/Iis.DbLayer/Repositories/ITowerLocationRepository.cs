using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel;

namespace Iis.DbLayer.Repositories
{
    public interface ITowerLocationRepository
    {
        Task<List<TowerLocationEntity>> GetAllAsync();
        Task<(decimal Lat, decimal Long)> GetByCellGlobalIdentityAsync(string mcc, string mnc, string lac, string cellId);
    }
}