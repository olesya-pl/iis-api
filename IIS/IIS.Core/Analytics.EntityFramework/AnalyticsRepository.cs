using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Analytics.EntityFramework
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private OntologyContext _dbCtx;
        public AnalyticsRepository(OntologyContext ctx)
        {
            _dbCtx = ctx;
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
                                 AND i.""ParentId"" is NOT NULL
                    )
                    SELECT * FROM children
                    ORDER BY level DESC
                    LIMIT 1
                ", childId)
                .FirstOrDefaultAsync();
        }
    }
}