using System.Collections.Generic;
using FluentAssertions;
using Iis.DbLayer.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class MultiSearchParamsQueryBuilderTests
    {
        [Fact]
        public void ExactIds_QueryString()
        {
            var sut = new MultiSearchParamsQueryBuilder(new List<SearchParameter> {
                new SearchParameter("__coordinates:* AND тестовий підрозділ 452", new List<IIisElasticField>
                {
                    new IisElasticField
                    {
                        Name = "purpose"
                    },
                    new IisElasticField
                    {
                        Name = "destination"
                    }
                }, true),
                new SearchParameter("1911159e0fb345fb9c8ac941ef674b5c 1a28d7a1dc1b476aa1fc10799ecf1a33", new List<IIisElasticField>
                {
                    new IisElasticField
                    {
                        Name = "Id",
                        Boost = 0.05m
                    }
                })
            })
                .WithPagination(0, 50).WithLeniency(true).WithResultFields(new[] { "*" }).BuildSearchQuery();

            var expected = JObject.Parse(@"{
  ""_source"": [
    ""*""
  ],
  ""from"": 0,
  ""size"": 50,
  ""query"": {
    ""bool"": {
        ""should"": [
        {
          ""query_string"": {
            ""query"": ""__coordinates:* AND тестовий підрозділ 452"",
            ""lenient"": true
          }
        },
        {
         ""query_string"": {
           ""query"": ""\""1911159e0fb345fb9c8ac941ef674b5c 1a28d7a1dc1b476aa1fc10799ecf1a33\"""",
           ""fuzziness"": 0,
           ""boost"": 0.05,
           ""lenient"": true,
           ""fields"": [
             ""Id""
           ]
         }
       }
    ]
    }
  }
}");
            sut.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ExactIds_QueryString_CountQuery()
        {
            var sut = new MultiSearchParamsQueryBuilder(new List<SearchParameter>
            {
                new SearchParameter("__coordinates:* AND тестовий підрозділ 452", new List<IIisElasticField>
                {
                    new IisElasticField
                    {
                        Name = "purpose"
                    },
                    new IisElasticField
                    {
                        Name = "destination"
                    }
                }, true),
                new SearchParameter("1911159e0fb345fb9c8ac941ef674b5c 1a28d7a1dc1b476aa1fc10799ecf1a33", new List<IIisElasticField>
                {
                    new IisElasticField
                    {
                        Name = "Id",
                        Boost = 0.05m
                    }
                })
            })
                .WithLeniency(true).BuildCountQuery();
            var expected = JObject.Parse(@"{
  ""query"": {
    ""bool"": {
        ""should"": [
        {
          ""query_string"": {
            ""query"": ""__coordinates:* AND тестовий підрозділ 452"",
            ""lenient"": true
          }
        },
        {
         ""query_string"": {
           ""query"": ""\""1911159e0fb345fb9c8ac941ef674b5c 1a28d7a1dc1b476aa1fc10799ecf1a33\"""",
           ""fuzziness"": 0,
           ""boost"": 0.05,
           ""lenient"": true,
           ""fields"": [
             ""Id""
           ]
         }
       }
    ]
    }
  }
}");
            sut.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ExactIds_QueryString_Highlights_Aggregations()
        {
            var sut = new MultiSearchParamsQueryBuilder(new List<SearchParameter>
            {
                new SearchParameter("__coordinates:* AND тестовий підрозділ 452", new List<IIisElasticField>
                {
                    new IisElasticField
                    {
                        Name = "purpose"
                    },
                    new IisElasticField
                    {
                        Name = "destination"
                    }
                }, true),
                new SearchParameter("1911159e0fb345fb9c8ac941ef674b5c 1a28d7a1dc1b476aa1fc10799ecf1a33", new List<IIisElasticField>
                {
                    new IisElasticField
                    {
                        Name = "Id",
                        Boost = 0.05m
                    }
                })
            })
                .WithPagination(0, 50)
                .WithLeniency(true)
                .WithResultFields(new[] { "*" })
                .BuildSearchQuery()
                .WithHighlights()
                .WithAggregation(new[] {
                    new AggregationField("affiliation.name", "Приналежність", "affiliation.nameAggregate"),
                    new AggregationField("importance.name", null, "importance.nameAggregate")
                });

            var expected = JObject.Parse(@"{
  ""_source"": [
    ""*""
  ],
  ""from"": 0,
  ""size"": 50,
  ""query"": {
    ""bool"": {
        ""should"": [
        {
          ""query_string"": {
            ""query"": ""__coordinates:* AND тестовий підрозділ 452"",
            ""lenient"": true
          }
        },
        {
         ""query_string"": {
           ""query"": ""\""1911159e0fb345fb9c8ac941ef674b5c 1a28d7a1dc1b476aa1fc10799ecf1a33\"""",
           ""fuzziness"": 0,
           ""boost"": 0.05,
           ""lenient"": true,
           ""fields"": [
             ""Id""
           ]
         }
       }
    ]
    }
  },
  ""highlight"": {
      ""fields"": {
        ""*"": {
          ""type"": ""unified""
        }
      }
    },
  ""aggs"": {
      ""Приналежність"": {
        ""terms"": {
          ""field"": ""affiliation.nameAggregate"",
          ""missing"": ""__hasNoValue"",
          ""size"": 100
        }
    },
    ""importance.name"": {
       ""terms"": {
        ""field"": ""importance.nameAggregate"",
        ""missing"": ""__hasNoValue"",
        ""size"": 100
       }
    }
  }
}");
            sut.Should().BeEquivalentTo(expected);
        }
    }
}
