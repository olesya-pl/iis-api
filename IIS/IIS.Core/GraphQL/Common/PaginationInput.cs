using System.Collections.Generic;
using System.Linq;
using HotChocolate.Execution;

namespace IIS.Core.GraphQL.Common
{
    public class PaginationInput
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public static class PaginationExtension
    {
        // Todo: Think about internal pagination in HotChocolate
        public static IEnumerable<T> Apply<T>(this PaginationInput pagination, IEnumerable<T> enumerable)
        {
            if (pagination.Page == 0) throw new QueryException("Nobody likes 0-based pagination :(");
            return enumerable.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize);
        }
    }
}
