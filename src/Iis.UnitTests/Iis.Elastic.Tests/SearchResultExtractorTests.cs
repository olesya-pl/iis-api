using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using Elasticsearch.Net;
using Iis.Elastic.SearchResult;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SearchResultExtractorTests
    {
        [Theory]
        [AutoData]
        public void GetFromReponse_ResultExists(Guid id1, Guid id2,
            string fileName1, string fileName2)
        {
            //arrange
            var apiCallMock = new Mock<IApiCallDetails>(MockBehavior.Strict);
            apiCallMock.Setup(e => e.HttpStatusCode).Returns(200);
            apiCallMock.Setup(e => e.Success).Returns(true);
            apiCallMock.Setup(e => e.AuditTrail).Returns(new List<Audit>());

            //act
            var sut = new SearchResultExtractor();
            var res = sut.GetFromResponse(new StringResponse(
                $@"{{
   ""took"":16,
   ""timed_out"":false,
   ""_shards"":{{
      ""total"":1,
      ""successful"":1,
      ""skipped"":0,
      ""failed"":0
   }},
   ""hits"":{{
      ""total"":{{
         ""value"":2,
         ""relation"":""eq""
      }},
      ""max_score"":5.53961,
      ""hits"":[
         {{
            ""_index"":""ont_materials"",
            ""_type"":""_doc"",
            ""_id"":""{id1.ToString("N")}"",
            ""_score"":5.53961,
            ""_source"":{{
               ""fileName"":""{fileName1}"",
            }},
            ""highlight"":{{
               ""source"":[
                  ""<em>sat.voice</em>""
               ],
            }}
         }},
         {{
            ""_index"":""ont_materials"",
            ""_type"":""_doc"",
            ""_id"":""{id2.ToString("N")}"",
            ""_score"":5.53961,
            ""_source"":{{
               ""fileName"":""{fileName2}"",
            }},
            ""highlight"":{{
               ""content"":[
                  ""Нех...й выгружаться, надо <em>бежать</em>""
               ]
            }}
         }}
      ]
   }}
}}"
                )
            {
                ApiCall = apiCallMock.Object
            });

            //assert
            Assert.Equal(2, res.Count);

            var item1 = res.Items.First(p => p.Identifier == id1.ToString("N"));
            Assert.Equal(fileName1, item1.SearchResult["fileName"]);
            Assert.Equal("<em>sat.voice</em>", item1.Higlight["source"][0]);

            var item2 = res.Items.First(p => p.Identifier == id2.ToString("N"));
            Assert.Equal(fileName2, item2.SearchResult["fileName"]);
            Assert.Equal("Нех...й выгружаться, надо <em>бежать</em>", item2.Higlight["content"][0]);
        }

        [Theory]
        [AutoData]
        public void GetFromReponse_Should_Unwrap_Grouped_Aggregations(Guid id1, Guid id2,
            string fileName1, string fileName2)
        {
            //arrange
            var apiCallMock = new Mock<IApiCallDetails>(MockBehavior.Strict);
            apiCallMock.Setup(e => e.HttpStatusCode).Returns(200);
            apiCallMock.Setup(e => e.Success).Returns(true);
            apiCallMock.Setup(e => e.AuditTrail).Returns(new List<Audit>());

            //act
            var sut = new SearchResultExtractor();
            var res = sut.GetFromResponse(new StringResponse(
                @"{ ""aggregations"": {
        ""foreignLang.lang.nameAggregate"": {
                ""doc_count"": 113,
            ""sub_aggs"": {
                    ""doc_count_error_upper_bound"": 0,
                ""sum_other_doc_count"": 0,
                ""buckets"": [
                    {
                        ""key"": ""__hasNoValue"",
                        ""doc_count"": 113
                    }
                ]
            }
            },
        ""GroupedAggregation7a98108ca54d4834821168739659f7a0"": {
                ""doc_count"": 113,
            ""importance.nameAggregate"": {
                    ""doc_count_error_upper_bound"": 0,
                ""sum_other_doc_count"": 0,
                ""buckets"": [
                    {
                        ""key"": ""першочерговий"",
                        ""doc_count"": 41
                    },
                    {
                        ""key"": ""підвищений"",
                        ""doc_count"": 24
                    },
                    {
                        ""key"": ""звичайний"",
                        ""doc_count"": 22
                    },
                    {
                        ""key"": ""важливий"",
                        ""doc_count"": 12
                    },
                    {
                        ""key"": ""низький"",
                        ""doc_count"": 8
                    },
                    {
                        ""key"": ""ігнорувати"",
                        ""doc_count"": 6
                    }
                ]
            },
            ""Приналежність"": {
                    ""doc_count_error_upper_bound"": 0,
                ""sum_other_doc_count"": 0,
                ""buckets"": [
                    {
                        ""key"": ""ворожий"",
                        ""doc_count"": 84
                    },
                    {
                        ""key"": ""підозрілий"",
                        ""doc_count"": 18
                    },
                    {
                        ""key"": ""умовно дружній"",
                        ""doc_count"": 5
                    },
                    {
                        ""key"": ""очікує розгляду"",
                        ""doc_count"": 4
                    },
                    {
                        ""key"": ""дружній"",
                        ""doc_count"": 1
                    },
                    {
                        ""key"": ""нейтральний"",
                        ""doc_count"": 1
                    }
                ]
            }
            }
        }}")
            {
                ApiCall = apiCallMock.Object
            });

            //assert
            Assert.Equal(3, res.Aggregations.Count);

            Assert.True(res.Aggregations.ContainsKey("foreignLang.lang.nameAggregate")
                && res.Aggregations.ContainsKey("importance.nameAggregate")
                && res.Aggregations.ContainsKey("Приналежність"));
        }

        [Fact]
        public void GetFromReponse_BadRequest()
        {
            //arrange
            var apiCallMock = new Mock<IApiCallDetails>(MockBehavior.Strict);
            apiCallMock.Setup(e => e.HttpStatusCode).Returns(500);
            apiCallMock.Setup(e => e.Success).Returns(false);
            apiCallMock.Setup(e => e.AuditTrail).Returns(new List<Audit>());
            apiCallMock.Setup(e => e.OriginalException).Returns(new Exception("hoba"));

            //act
            var sut = new SearchResultExtractor();

            //assert
            Assert.Throws<Exception>(() => sut.GetFromResponse(new StringResponse("{\"error\":{\"root_cause\":[{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\"}],\"type\":\"search_phase_execution_exception\",\"reason\":\"all shards failed\",\"phase\":\"query\",\"grouped\":true,\"failed_shards\":[{\"shard\":0,\"index\":\"ont_materials\",\"node\":\"_L87vs7ZSlW2XA6AstCxfg\",\"reason\":{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\"}}],\"caused_by\":{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\",\"caused_by\":{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\"}}},\"status\":400}")
            {
                ApiCall = apiCallMock.Object
            }));
        }
    }
}
