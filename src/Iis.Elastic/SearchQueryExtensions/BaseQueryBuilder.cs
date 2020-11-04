using System.Collections.Generic;

namespace Iis.Elastic.SearchQueryExtensions
{
    public abstract class BaseQueryBuilder<T> where T : BaseQueryBuilder<T>
    {
        protected IReadOnlyCollection<string> _resultFields = new[] { "*" };
        protected int _offset;
        protected int _limit;

        public T WithPagination(int offset, int limit)
        {
            _offset = offset;
            _limit = limit;
            return this as T;
        }

        public T WithResultFields(IReadOnlyCollection<string> resultFields)
        {
            _resultFields = resultFields;
            return this as T;
        }
    }
}