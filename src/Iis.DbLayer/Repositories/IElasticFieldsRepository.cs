using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.Elastic;
using Iis.Interfaces.Elastic;

namespace Iis.DbLayer.Repositories
{
    public interface IElasticFieldsRepository
    {
        Task<Dictionary<string, ElasticFieldEntity>> GetElasticFieldsByTypename(string typeName);
        Task<List<ElasticFieldEntity>> GetReadonlyElasticFieldsByTypename(string typeName);
        Task AddField(List<ElasticFieldEntity> entitiesToAdd, List<ElasticFieldEntity> entititesToUpdate);
    }
}