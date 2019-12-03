using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology;

namespace IIS.Core.Analytics.EntityFramework
{
    public interface IAnalyticsRepository
    {
        Task<AnalyticsIndicator> getRootAsync(Guid childId);
        Task<IEnumerable<AnalyticsIndicator>> GetAllChildrenAsync(Guid parentId);
        Task<IEnumerable<AnalyticsQueryIndicatorResult>> calcAsync(AnalyticsQueryBuilder query);
        Task<IEnumerable<AnalyticsQueryIndicatorResult>> calcAsync(AnalyticsIndicator indicator, DateTime? fromDate, DateTime? toDate);
        Task<AnalyticsQueryBuilder> BuildQuery(AnalyticsIndicator indicator, DateTime? from, DateTime? to);
    }
}