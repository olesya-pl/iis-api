using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Contracts.Repositories;

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
            "PermanentCoefficient",
            "SecurityLevels"
        };
        private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true };

        private readonly IMaterialElasticRepository _materialElasticRepository;

        public MaterialDisributionService(
            IMaterialElasticRepository materialElasticRepository)
        {
            _materialElasticRepository = materialElasticRepository;
        }

        public async Task<IReadOnlyCollection<MaterialDistributionInfo>> GetMaterialCollectionAsync(int offsetHours, CancellationToken cancellationToken)
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
                .Select(_ => JsonSerializer.Deserialize<MaterialDistributionInfo>(_.SearchResult.ToString(), _defaultJsonSerializerOptions))
                .ToArray();
        }

        private static string GetSuggestion(int hourOffset)
        {
            var dateRangeExpression = $"now-{hourOffset}h\\/h";

            return $"(ParentId:NULL)AND((CreatedDate:>{dateRangeExpression})OR(RegistrationDate:>{dateRangeExpression}))";
        }
    }
}