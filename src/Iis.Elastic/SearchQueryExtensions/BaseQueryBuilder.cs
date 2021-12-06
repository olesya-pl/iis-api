using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Elastic.Dictionaries;

namespace Iis.Elastic.SearchQueryExtensions
{
    public abstract class BaseQueryBuilder<T>
    where T : BaseQueryBuilder<T>
    {
        protected IReadOnlyCollection<string> _sourceCollection = SearchQueryExtension.DefaultSourceCollectionValue;

        public T WithResultFields(IReadOnlyCollection<string> sourceCollection)
        {
            _sourceCollection = sourceCollection;

            return this as T;
        }

        public JObject BuildSearchQuery()
        {
            var jsonQuery = GetBaseQuery();

            return CreateQuery(jsonQuery);
        }

        public JObject BuildCountQuery()
        {
            var json = new JObject(new JProperty(SearchQueryPropertyName.Query, new JObject()));

            return CreateQuery(json);
        }

        protected virtual JObject GetBaseQuery() => SearchQueryExtension.GetBaseQueryJson(_sourceCollection);

        protected abstract JObject CreateQuery(JObject json);
    }
}