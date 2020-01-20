using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Iis.DataModel.Analytics;

namespace IIS.Core.Analytics.EntityFramework
{
    public interface IAnalyticsRepository
    {
        Task<AnalyticIndicatorEntity> getRootAsync(Guid childId);
        Task<IEnumerable<AnalyticIndicatorEntity>> GetAllChildrenAsync(Guid parentId);
        Task<IEnumerable<AnalyticsQueryIndicatorResult>> calcAsync(AnalyticsQueryBuilder query);
        Task<IEnumerable<AnalyticsQueryIndicatorResult>> calcAsync(AnalyticIndicatorEntity indicator, DateTime? fromDate, DateTime? toDate);
        Task<AnalyticsQueryBuilder> BuildQuery(AnalyticIndicatorEntity indicator, DateTime? from, DateTime? to);
    }
}