using System;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Analytics.EntityFramework;
using Newtonsoft.Json.Linq;

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
        public async Task<IEnumerable<AnalyticsQueryIndicatorResult>> GetValues([Service] IOntologyProvider ontologyProvider, [Service] IAnalyticsRepository repository)
        {
            if (_indicator.Query == null)
                return null;

            var ontology = await ontologyProvider.GetOntologyAsync();
            var query = _getIndicatorQuery(ontology);
            return await repository.calcAsync(query);
        }

        private AnalyticsQueryBuilder _getIndicatorQuery(Ontology.Ontology ontology)
        {
            var query = @"
                {
                    ""Match"": [
                        ""(o:Organization)-[:srddw]->(s:Srddw)"",
                        ""(o)-[:dotl]->(sbu:Organization)"",
                        ""(s)-[:startDate]->@(startDate:Date)""
                    ],
                    ""GroupBy"": [""sbu.Id""],
                    ""Count"": ""s.Id"",
                    ""Conditions"": [
                        {
                            ""Field"": ""startDate.Value"",
                            ""Op"": "">"",
                            ""Value"": ""2019-10-11T10:12:00""
                        }
                    ]
                }
            ";
            AnalyticsQueryBuilderConfig config;
            try {
                config = JObject.Parse(query).ToObject<AnalyticsQueryBuilderConfig>();
            } catch {
                throw new InvalidOperationException($"Query of \"{Title}\" analytics Indicator is invalid");
            }

            return AnalyticsQueryBuilder.From(ontology).Load(config);
        }
    }
}