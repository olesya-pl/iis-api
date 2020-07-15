using System;
using System.Linq;
using AutoFixture.Xunit2;
using Elasticsearch.Net;
using Iis.Elastic;
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
            //act
            var sut = new SearchResultExtractor();
            var res = sut.GetFromResponse(new StringResponse(
                @$"{{
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
                ));

            //assert
            Assert.Equal(2, res.Count);

            var item1 = res.Items.First(p => p.Identifier == id1.ToString("N"));
            Assert.Equal(fileName1, item1.SearchResult["fileName"]);
            Assert.Equal("<em>sat.voice</em>", item1.Higlight["source"][0]);

            var item2 = res.Items.First(p => p.Identifier == id2.ToString("N"));
            Assert.Equal(fileName2, item2.SearchResult["fileName"]);
            Assert.Equal("Нех...й выгружаться, надо <em>бежать</em>", item2.Higlight["content"][0]);
        }

        [Fact]
        public void GetFromReponse_BadRequest()
        {
            //act
            var sut = new SearchResultExtractor();
            var res = sut.GetFromResponse(new StringResponse("{\"error\":{\"root_cause\":[{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\"}],\"type\":\"search_phase_execution_exception\",\"reason\":\"all shards failed\",\"phase\":\"query\",\"grouped\":true,\"failed_shards\":[{\"shard\":0,\"index\":\"ont_materials\",\"node\":\"_L87vs7ZSlW2XA6AstCxfg\",\"reason\":{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\"}}],\"caused_by\":{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\",\"caused_by\":{\"type\":\"illegal_argument_exception\",\"reason\":\"boost must be a positive float, got -1.0\"}}},\"status\":400}"));

            //assert
            Assert.Equal(0, res.Count);
        }
    }
}
