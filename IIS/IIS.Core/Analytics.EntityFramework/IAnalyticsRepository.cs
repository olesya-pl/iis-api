using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core.Analytics.EntityFramework
{
    public interface IAnalyticsRepository
    {
        Task<AnalyticsIndicator> getRootAsync(Guid childId);
        Task<IEnumerable<AnalyticsIndicator>> GetAllChildrenAsync(Guid parentId);
    }
}