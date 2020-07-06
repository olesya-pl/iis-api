using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Iis.Interfaces.Repository;

namespace Iis.DataModel.Materials
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly IDbConnection _connection;

        public MaterialRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public Task<int> GetCountOfParentMaterials(Guid[] materialIds)
        {
            const string sql =
                @"select count(""Id"")
                    from public.""Materials""
                    where ""ParentId"" is null and ""Id"" IN @ids";
            return _connection.QuerySingleAsync<int>(sql, new { ids = materialIds });
        }
    }
}
