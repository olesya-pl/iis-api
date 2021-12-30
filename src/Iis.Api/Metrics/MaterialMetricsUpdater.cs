using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Interfaces.Metrics;
using Iis.Metrics.Materials;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Iis.Api.Metrics
{
    public class MaterialMetricsUpdater
    {
        private readonly ILogger<MaterialMetricsManager> _logger;
        private readonly OntologyContext _ontologyContext;
        private readonly IMaterialMetricsManager _materialMetricsManager;

        public MaterialMetricsUpdater(
            ILogger<MaterialMetricsManager> logger,
            OntologyContext ontologyContext,
            IMaterialMetricsManager materialMetricsManager)
        {
            _logger = logger;
            _ontologyContext = ontologyContext;
            _materialMetricsManager = materialMetricsManager;
        }

        public async Task UpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var data = await _ontologyContext.Materials
                    .AsNoTracking()
                    .Where(_ => _.ParentId == null)
                    .GroupBy(_ => _.Source)
                    .Select(_ => new
                    {
                        Type = _.Key,
                        Count = _.LongCount()
                    })
                    .ToArrayAsync(cancellationToken);
                var materialMetrics = new MaterialMetrics
                {
                    TotalCount = data.Sum(_ => _.Count),
                    BySourceCounts = data.ToDictionary(_ => _.Type, _ => _.Count)
                };

                _materialMetricsManager.SetMetrics(materialMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot update material metrics: {ex.Message}");
                throw;
            }
        }
    }
}