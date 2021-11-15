using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders.BoolQuery
{
    public class CompositeBoolQueryBuilder : BaseQueryBuilder<CompositeBoolQueryBuilder>
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
            var conditionDictionary = BuildConditions()
                .GroupBy(_ => _.Occur)
                .ToDictionary(_ => _.Key, _ => _.Select(_ => _.Condition));

            foreach (var (occur, conditions) in conditionDictionary)
            {
                boolClause[occur] = new JArray(conditions);
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
    }
}