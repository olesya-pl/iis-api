using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Microsoft.EntityFrameworkCore;
using Iis.DataModel;
using Iis.DataModel.Analytics;
using Newtonsoft.Json;
using IIS.Domain;
using Iis.DbLayer.Ontology.EntityFramework;

namespace IIS.Core.Analytics.EntityFramework
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private OntologyContext _dbCtx;
        private ContextFactory _ctxFactory;
        private IOntologyProvider _ontologyProvider;

        public AnalyticsRepository(OntologyContext ctx, ContextFactory contextFactory, IOntologyProvider ontologyProvider)
        {
            _dbCtx = ctx;
            _ctxFactory = contextFactory;
            _ontologyProvider = ontologyProvider;
        }

        public async Task<IEnumerable<AnalyticIndicatorEntity>> GetAllChildrenAsync(Guid parentId)
        {
            // TODO: better to use QueryBuilder because escaping is Postgres specific
            return await _dbCtx.AnalyticIndicators
                .FromSqlRaw(@"
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

        public async Task<AnalyticIndicatorEntity> getRootAsync(Guid childId)
        {
            // TODO: better to use QueryBuilder because escaping is Postgres specific
            return await _dbCtx.AnalyticIndicators
                .FromSqlRaw(@"
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

        public async Task<IEnumerable<AnalyticsQueryIndicatorResult>> calcAsync(AnalyticIndicatorEntity indicator, DateTime? fromDate, DateTime? toDate)
        {
            var query = await BuildQuery(indicator, fromDate, toDate);
            return await calcAsync(query);
        }

        public async Task<AnalyticsQueryBuilder> BuildQuery(AnalyticIndicatorEntity indicator, DateTime? fromDate, DateTime? toDate)
        {
            AnalyticsQueryBuilderConfig config;
            try {
                config = JsonConvert.DeserializeObject<AnalyticsQueryBuilderConfig>(indicator.Query);
            } catch {
                throw new InvalidOperationException($"Query of \"{indicator.Title}\" analytics Indicator is invalid");
            }

            var ontology = await _ontologyProvider.GetOntologyAsync();
            var finalQuery = AnalyticsQueryBuilder.From(ontology).Load(config);
            var (startDateField, endDateField) = (config.StartDateField, config.EndDateField ?? config.StartDateField);

            if (startDateField != null && fromDate != null)
                finalQuery.Where(startDateField, ">=", fromDate);

            if (endDateField != null && toDate != null)
                finalQuery.Where(endDateField, "<=", toDate);

            return finalQuery;
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
