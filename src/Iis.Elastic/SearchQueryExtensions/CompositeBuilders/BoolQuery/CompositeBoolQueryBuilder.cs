using Iis.Elastic.Dictionaries;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders.BoolQuery
{
    public class CompositeBoolQueryBuilder : PaginatedQueryBuilder<CompositeBoolQueryBuilder>
    {
        private readonly List<IQueryConditionBuilder> _conditionBuilders = new List<IQueryConditionBuilder>();

        public CompositeBoolQueryBuilder WithCondition<TConditionBuilder>(Action<TConditionBuilder> configureAction)
            where TConditionBuilder : BaseQueryConditionBuilder<TConditionBuilder>, new()
        {
            if (configureAction is null) return this;

            var conditionBuilder = new TConditionBuilder();

            configureAction(conditionBuilder);
            _conditionBuilders.Add(conditionBuilder);

            return this;
        }

        protected override JObject CreateQuery(JObject jsonQuery)
        {
            var boolClause = new JObject();
            var conditions = BuildConditions();

            foreach (var (occur, items) in GroupConditions(conditions))
            {
                boolClause[occur] = new JArray(items);
            }

            jsonQuery[QueryTerms.Conditions.Query][QueryTerms.Conditions.Bool] = boolClause;

            return jsonQuery;
        }

        private IEnumerable<(string Occur, JObject Condition)> BuildConditions()
        {
            foreach (var conditionBuilder in _conditionBuilders)
            {
                var condition = conditionBuilder.Build();
                if (condition is null) continue;

                yield return (conditionBuilder.Occur, condition);
            }
        }

        private IEnumerable<(string Occur, IEnumerable<JObject> Conditions)> GroupConditions(IEnumerable<(string Occur, JObject Condition)> conditions)
        {
            var conditionDictionary = conditions.GroupBy(_ => _.Occur)
                .ToDictionary(_ => _.Key, _ => _.Select(_ => _.Condition));
            var joinRelatedConditions = conditionDictionary.ContainsKey(QueryBooleanOccurs.Should)
                && conditionDictionary.ContainsKey(QueryBooleanOccurs.Must);
            if (joinRelatedConditions)
            {
                var mustConditions = conditionDictionary[QueryBooleanOccurs.Must];
                var shouldConditions = conditionDictionary[QueryBooleanOccurs.Should];

                yield return (QueryBooleanOccurs.Must, mustConditions.Concat(shouldConditions));
            }

            var remainedConditions = joinRelatedConditions
                ? conditionDictionary.Where(_ => _.Key != QueryBooleanOccurs.Should && _.Key != QueryBooleanOccurs.Must)
                : conditionDictionary;

            foreach (var (occur, items) in remainedConditions)
            {
                yield return (occur, items);
            }
        }
    }
}