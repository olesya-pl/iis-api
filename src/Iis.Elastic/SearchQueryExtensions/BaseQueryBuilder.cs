using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public abstract class BaseQueryBuilder<T> where T : BaseQueryBuilder<T>
    {
        protected const string QueryPropertyName = "query";
        protected const string FromPropertyName = "from";
        protected const string SizePropertyName = "size";
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
        
        public JObject BuildSearchQuery() {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _from, _size);
            jsonQuery["query"] = new JObject();

            return CreateQuery(jsonQuery);
        }

        public JObject BuildCountQuery() 
        {
            var json = new JObject(new JProperty("query", new JObject()));
            return CreateQuery(json);
        }
        
        protected abstract JObject CreateQuery(JObject json);
    }
}