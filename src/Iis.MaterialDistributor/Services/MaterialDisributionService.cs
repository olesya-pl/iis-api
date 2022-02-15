using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Iis.Utility;
using Iis.Interfaces.Elastic;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Services
{
    internal class MaterialDisributionService : IMaterialDistributionService
    {
        private static readonly PaginationParams _defaultPagination = new PaginationParams(1, 10000);
        private static readonly string[] _resultFieldCollection =
        {
            "Id",
            "Channel",
            "RegistrationDate",
            "CreatedDate",
            "SecurityLevels"
        };

        private readonly IMaterialElasticRepository _materialElasticRepository;

        public MaterialDisributionService(
            IMaterialElasticRepository materialElasticRepository)
        {
            _materialElasticRepository = materialElasticRepository;
        }

        public async Task<List<MaterialDistributionInfo>> GetMaterialCollectionAsync(int offsetHours, CancellationToken cancellationToken)
        {
            var searchParam = new SearchParams(GetSuggestion(offsetHours), _defaultPagination, _resultFieldCollection);

            var result = await _materialElasticRepository.BeginSearchByScrollAsync(searchParam, cancellationToken);

            var resultCollection = new List<MaterialDistributionInfo>(result.Count);

            resultCollection.AddRange(MapSearchResultToMaterialDocumentCollections(result.Items.Values));

            while (result.Items.Count > 0)
            {
                result = await _materialElasticRepository.SearchByScrollAsync(result.ScrollId, cancellationToken);

                resultCollection.AddRange(MapSearchResultToMaterialDocumentCollections(result.Items.Values));
            }

            return resultCollection;
        }

        private static IReadOnlyList<MaterialDistributionInfo> MapSearchResultToMaterialDocumentCollections(IReadOnlyCollection<SearchResultItem> searchResultItemCollection)
        {
            return searchResultItemCollection
                .Select(_ => JsonSerializer.Deserialize<MaterialDistributionInfo>(_.SearchResult.ToString()))
                .ToArray();
        }

        private static string GetSuggestion(int hourOffset)
        {
            /*
            // temporally. for dev purposes only
            var timeStamp = new DateTime(2021, 12, 13, 18, 02, 13);
            var escapeSymbolsPattern = new HashSet<char> { ':', '/' };
            var dateRangeExpression = $"{timeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ")}||-{hourOffset}h/h".EscapeSymbols(escapeSymbolsPattern);
            */

            var dateRangeExpression = $"now-{hourOffset}h\\/h";

            return $"(ParentId:NULL)AND((CreatedDate:>{dateRangeExpression})OR(RegistrationDate:>{dateRangeExpression}))";
        }
    }
}