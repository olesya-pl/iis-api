using System.Collections.Generic;
using Iis.Interfaces.Elastic;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public class SearchParams
    {
        public SearchParams(string suggestion, PaginationParams pagination, IReadOnlyCollection<string> resultFieldCollection)
        {
            Suggestion = suggestion;
            Pagination = pagination;
            ResultFieldCollection = resultFieldCollection;
        }

        public string Suggestion { get; }
        public PaginationParams Pagination { get; }
        public IReadOnlyCollection<string> ResultFieldCollection { get; }
    }
}