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

namespace Iis.MaterialDistributor.Services
{
    internal class MaterialService : IMaterialService
    {
        private static readonly PaginationParams _defaultPagination = new PaginationParams(1, 10000);
        private static readonly string[] _resultFieldCollection =
        {
            "Id",
            "Channel",
            "RegistrationDate",
            "CreatedDate"
        };

        private IMaterialElasticRepository _elasticRepository;

        public MaterialService(
            IMaterialElasticRepository elasticRepository)
        {
            _elasticRepository = elasticRepository;
        }

        public async Task<IReadOnlyCollection<MaterialDocument>> GetMaterialCollectionAsync(int hourOffset, CancellationToken cancellationToken)
        {
            var searchParam = new SearchParams(GetSuggestion(hourOffset), _defaultPagination, _resultFieldCollection);

            var result = await _elasticRepository.BeginSearchByScrollAsync(searchParam, cancellationToken);

            var resultCollection = new List<MaterialDocument>(result.Count);

            resultCollection.AddRange(MapSearchResultToMaterialDocumentCollections(result.Items.Values));

            while (result.Items.Count > 0)
            {
                result = await _elasticRepository.SearchByScrollAsync(result.ScrollId, cancellationToken);

                resultCollection.AddRange(MapSearchResultToMaterialDocumentCollections(result.Items.Values));
            }

            return resultCollection;
        }

        private static IReadOnlyCollection<MaterialDocument> MapSearchResultToMaterialDocumentCollections(IReadOnlyCollection<SearchResultItem> searchResultItemCollection)
        {
            return searchResultItemCollection
                .Select(_ => JsonSerializer.Deserialize<MaterialDocument>(_.SearchResult.ToString()))
                .ToArray();
        }

        private static string GetSuggestion(int hourOffset)
        {
            //var timeStamp = new DateTime(2021, 12, 13, 18, 02, 13);
            //var escapeSymbolsPattern = new HashSet<char> { ':', '/' };
            //var dateRangeExpression = $"{timeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ")}||-{hourOffset}h/h".EscapeSymbols(escapeSymbolsPattern);

            var dateRangeExpression = $"now-{hourOffset}h\\/h";

            return $"(ParentId:NULL)AND((CreatedDate:>{dateRangeExpression})OR(RegistrationDate:>{dateRangeExpression}))";
        }
    }
}