using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using IIS.Core.Ontology.EntityFramework;

namespace IIS.Core.Analytics.EntityFramework
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private OntologyContext _dbCtx;
        private ContextFactory _ctxFactory;

        public AnalyticsRepository(OntologyContext ctx, ContextFactory contextFactory)
        {
            _dbCtx = ctx;
            _ctxFactory = contextFactory;
        }

        public async Task<IEnumerable<AnalyticsIndicator>> GetAllChildrenAsync(Guid parentId)
        {
            // TODO: better to use QueryBuilder because escaping is Postgres specific
            return await _dbCtx.AnalyticsIndicators
                .FromSql(@"
                    WITH RECURSIVE children AS (
                        SELECT *
                        FROM ""AnalyticsIndicators""
                        WHERE ""ParentId"" = {0}
                        UNION
                        SELECT i.*
                        FROM ""AnalyticsIndicators"" i
                          INNER JOIN children c ON c.""Id"" = i.""ParentId""
                    )
                    SELECT * FROM children
                ", parentId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AnalyticsIndicator> getRootAsync(Guid childId)
        {
            // TODO: better to use QueryBuilder because escaping is Postgres specific
            return await _dbCtx.AnalyticsIndicators
                .FromSql(@"
                    WITH RECURSIVE children AS (
                        SELECT *, 0 as level
                        FROM ""AnalyticsIndicators""
                        WHERE ""Id"" = {0}
                        UNION
                        SELECT i.*, level + 1 as level
                        FROM ""AnalyticsIndicators"" i
                          INNER JOIN children c
                                  ON c.""ParentId"" = i.""Id""
                    )
                    SELECT * FROM children
                    ORDER BY level DESC
                    LIMIT 1
                ", childId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AnalyticsQueryIndicatorResult>> calcAsync(AnalyticsQueryBuilder query)
        {
            using (var ctx = _ctxFactory.CreateContext())
            {
                var (sql, sqlParams) = query.ToSQL();
                List<AnalyticsQueryIndicatorResult> results;

                using (var command = ctx.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    foreach (var param in sqlParams)
                    {
                        var parameter = command.CreateParameter();
                        parameter.Value = param.Value;
                        parameter.ParameterName = param.Key;
                        command.Parameters.Add(parameter);
                    }

                    ctx.Database.OpenConnection();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        results = new List<AnalyticsQueryIndicatorResult>();
                        while (reader.Read())
                            results.Add(new AnalyticsQueryIndicatorResult(reader.GetGuid(0), reader[1]));
                    }
                }

                return results;
            }
        }
    }

    public class AnalyticsQueryIndicatorResult
    {
        public Guid Id { get; set; }
        public object Value { get; set; }

        public AnalyticsQueryIndicatorResult(Guid entityId, object value)
        {
            Id = entityId;
            Value = value;
        }
    }
}