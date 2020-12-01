using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public abstract class BaseQueryBuilder<T> where T : BaseQueryBuilder<T>
    {
        protected IReadOnlyCollection<string> _resultFields = new[] { "*" };
        protected int _from;
        protected int _size;

        public T WithPagination(int from, int size)
        {
            _from = from;
            _size = size;
            return this as T;
        }

        public T WithResultFields(IReadOnlyCollection<string> resultFields)
        {
            _resultFields = resultFields;
            return this as T;
        }
        public abstract JObject Build();
    }
}