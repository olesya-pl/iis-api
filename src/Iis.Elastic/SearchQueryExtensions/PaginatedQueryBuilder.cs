using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public abstract class PaginatedQueryBuilder<T> : BaseQueryBuilder<PaginatedQueryBuilder<T>>
    where T : PaginatedQueryBuilder<T>
    {
        private int _from;
        private int _size;

        public T WithPagination(int from, int size)
        {
            _from = from;
            _size = size;

            return this as T;
        }

        protected override JObject GetBaseQuery() => SearchQueryExtension.GetPaginatedBaseQueryJson(_sourceCollection, _from, _size);
    }
}