using System;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Analytics.EntityFramework;
using Newtonsoft.Json;

namespace IIS.Core.GraphQL.AnalyticsIndicator
{
    public class AnalyticsIndicator
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }

        [GraphQLType(typeof(IdType))]
        public string ParentId { get; set; }

        private readonly IIS.Core.Analytics.EntityFramework.AnalyticsIndicator _indicator;

        public AnalyticsIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsIndicator indicator)
        {
            Id = indicator.Id.ToString();
            Title = indicator.Title;

            if (indicator.ParentId != null)
            {
                ParentId = indicator.ParentId.ToString();
            }

            _indicator = indicator;
        }

        [GraphQLType(typeof(AnyType))]
        public async Task<IEnumerable<AnalyticsQueryIndicatorResult>> GetValues([Service] IOntologyProvider ontologyProvider, [Service] IAnalyticsRepository repository, DateTime? from, DateTime? to)
        {
            if (_indicator.Query == null)
                return null;

            var ontology = await ontologyProvider.GetOntologyAsync();
            var query = _getIndicatorQuery(ontology, from, to);
            return await repository.calcAsync(query);
        }

        private AnalyticsQueryBuilder _getIndicatorQuery(Ontology.Ontology ontology, DateTime? fromDate, DateTime? toDate)
        {
            AnalyticsQueryBuilderConfig config;
            try {
                config = JsonConvert.DeserializeObject<AnalyticsQueryBuilderConfig>(_indicator.Query);
            } catch {
                throw new InvalidOperationException($"Query of \"{Title}\" analytics Indicator is invalid");
            }

            var query = AnalyticsQueryBuilder.From(ontology).Load(config);

            if (config.startDateField != null && fromDate != null)
                query.Where(config.startDateField, ">=", fromDate);

            if (config.endDateField != null && toDate != null)
                query.Where(config.endDateField, "<=", toDate);

            return query;
        }
    }
}