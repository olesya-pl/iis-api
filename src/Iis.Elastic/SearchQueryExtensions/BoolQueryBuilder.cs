using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Elastic.Dictionaries;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class BoolQueryBuilder : BaseQueryBuilder<BoolQueryBuilder>
    {
        private string _occur = QueryBooleanOccurs.Should;
        private string _query = string.Empty;
        private bool _isLenient = true;
        private IEnumerable<Guid> _documentList;

        public BoolQueryBuilder WithMust()
        {
            _occur = QueryBooleanOccurs.Must;
            return this;
        }

        public BoolQueryBuilder WithShould()
        {
            _occur = QueryBooleanOccurs.Should;
            return this;
        }

        public BoolQueryBuilder WithExactQuery(string query)
        {
            _query = query;

            return this;
        }

        public BoolQueryBuilder WithDocumentList(IEnumerable<Guid> documentList)
        {
            _documentList = documentList;
            return this;
        }

        public override JObject Build()
        {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _offset, _limit);

            var boolClause = new JObject();

            var occurQueries = new JArray();

            boolClause[_occur] = occurQueries;

            var documentListQuery = GetDocumentListQuery(_documentList);

            if(documentListQuery != null)
            {
                occurQueries.Add(documentListQuery);
            }

            var exactQuery = GetExactQuery(_query);

            if(exactQuery != null)
            {
                occurQueries.Add(exactQuery);
            }


            jsonQuery["query"] = new JObject(
                new JProperty("bool", boolClause)
            );

            return jsonQuery;
        }

        private JObject GetDocumentListQuery(IEnumerable<Guid> documentList)
        {
            if(documentList is null || !documentList.Any()) return null;

            var idList = documentList.Select(e => e.ToString("N"));

            var values = new JObject(
                new JProperty("values", new JArray(idList))
            );

            return new JObject(
                new JProperty("ids", values)
            );
        }

        private JObject GetExactQuery(string query)
        {
            if(string.IsNullOrWhiteSpace(query)) return null;

            var queryStringProperty = new JObject(
                new JProperty("query", query),
                new JProperty("lenient", _isLenient)
            );

            var queryString = new JObject(
                new JProperty("query_string", queryStringProperty)
            );

            return  queryString;
        }
    }
}