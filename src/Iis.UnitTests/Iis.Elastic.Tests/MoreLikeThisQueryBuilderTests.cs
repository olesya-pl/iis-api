using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class MoreLikeThisQueryBuilderTests
    {
        [Fact]
        public void WithPagination_Success()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {}}");

            var actual = new MoreLikeThisQueryBuilder()
                        .WithPagination(10, 10)
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(new []{"*"}, 1, 5)]
        [InlineData(new []{"Id", "Type", "Source"}, 0, 3)]
        public void WithResultFields_Success(IEnumerable<string> resultFieldList, int from, int size)
        {
            var expected = JObject.Parse("{\"_source\":["+string.Join(",", resultFieldList.Select(x => $"\"{x}\""))+"], \"from\":"+from+",\"size\":"+size+",\"query\": {}}");

            var actual = new MoreLikeThisQueryBuilder()
                        .WithResultFields(resultFieldList.ToList().AsReadOnly())
                        .WithPagination(from, size)
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WithMaterialId_Sucess()
        {
            var expected = JObject.Parse(
                @"{
                    '_source': ['*'],
                    'from':0,
                    'size':50,
                    'query':{
                        'bool':{
                            'must':[
                                {'term': {'ParentId':'NULL'}},
                                {'more_like_this': {
                                        'fields': [ 'Content' ],
                                        'like' : [ { '_id': 'f4eed773e59d49aba10f01c7fc4ca47f' } ],
                                        'min_term_freq' : 1
                                    }
                                }
                            ]
                        }
                    }
                }"
            );

            var actual = new MoreLikeThisQueryBuilder()
                        .WithPagination(0, 50)
                        .WithMaterialId("f4eed773e59d49aba10f01c7fc4ca47f")
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WithMaterial_NoId_EmptyQuery()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":0,\"size\":50,\"query\": {}}");

            var actual = new MoreLikeThisQueryBuilder()
                        .WithPagination(0, 50)
                        .WithMaterialId(string.Empty)
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}