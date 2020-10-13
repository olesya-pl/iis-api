using Iis.Domain.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class ReportElasticService : IReportElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly string _elasticIndex;
        private const int ElasticBulkSize = 10000;

        public ReportElasticService(IElasticManager elasticManager, IElasticState elasticState)
        {
            _elasticManager = elasticManager ?? throw new ArgumentNullException(nameof(elasticManager));
            _elasticIndex = elasticState?.ReportIndex;
        }

        public Task<bool> PutAsync(ReportDto report)
        {
            var jsonReport = JsonConvert.SerializeObject(report);
            return _elasticManager.PutDocumentAsync(_elasticIndex, report.Id.ToString("N"), jsonReport, true);
        }

        public Task<bool> RemoveAsync(Guid id)
        {
            return _elasticManager.DeleteDocumentAsync(_elasticIndex, id.ToString("N"));
        }

        public async Task<(int Count, List<ReportDto> Items)> SearchAsync(ReportSearchParams search) 
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = new List<string> { _elasticIndex },
                Query = string.IsNullOrEmpty(search.Suggestion) ? "*" : $"*{search.Suggestion}*",
                From = search.Offset,
                Size = search.PageSize,
            };

            if (!string.IsNullOrEmpty(search.SortColumn) && !string.IsNullOrEmpty(search.SortOrder)) 
            {
                searchParams.SortColumn = GetValidSortColumnName(search.SortColumn);
                searchParams.SortOrder = search.SortOrder;
            }

            var searchResult = await _elasticManager.Search(searchParams);
            return (searchResult.Count, searchResult.Items.Select(x => x.SearchResult.ToObject<ReportDto>()).ToList());
        }

        public async Task<ReportDto> GetAsync(Guid id) 
        {
            var document = await _elasticManager.GetDocumentByIdAsync(new string[] { _elasticIndex }, id.ToString("N"));
            var jsonReport = document.Items.FirstOrDefault()?.SearchResult;
            
            return jsonReport?.ToObject<ReportDto>();
        }

        private string GetValidSortColumnName(string sortColumn) 
        {
            var property = typeof(ReportDto)
                .GetProperties()
                .SingleOrDefault(x => string.Equals(x.Name, sortColumn, StringComparison.OrdinalIgnoreCase));

            if (property == null)
                throw new ArgumentException($"{sortColumn} is not valid property for sorting");

            return property.PropertyType == typeof(string) ? $"{property.Name}.keyword" : property.Name;
        }

        public async Task<List<ElasticBulkResponse>> PutAsync(IEnumerable<ReportDto> reports)
        {
            var responses = new List<ElasticBulkResponse>();
            foreach (var reportBatch in reports.Batch(ElasticBulkSize))
            {
                var jsonReports = reportBatch
                 .Aggregate("", (acc, r) => acc += $"{{ \"index\":{{ \"_id\": \"{r.Id:N}\" }} }}\n{JsonConvert.SerializeObject(r)}\n");
                
                responses.AddRange(await _elasticManager.PutDocumentsAsync(_elasticIndex, jsonReports));
            }

            return responses;
        }
    }
}
