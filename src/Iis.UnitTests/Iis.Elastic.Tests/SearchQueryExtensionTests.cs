using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SearchQueryExtensionTests
    {
        [Theory]
        [InlineData(new []{"*"}, 1, 5)]
        [InlineData(new []{"Id", "Type", "Source"}, 0, 3)]
        public void WithSearchJson_Success(IEnumerable<string> resultFieldList, int from, int size)
        {
            var expected = JObject.Parse("{\"_source\":["+string.Join(",", resultFieldList.Select(x => $"\"{x}\""))+"], \"from\":"+from+",\"size\":"+size+",\"query\": {}}");

            var actual = SearchQueryExtension.WithSearchJson(resultFieldList, from, size);

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData(1, 5)]
        public void WithSearchJson_DefaultResultFieldList(int from, int size)
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":"+from+",\"size\":"+size+",\"query\": {}}");

            var actual = SearchQueryExtension.WithSearchJson(null, from, size);

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupHighlights_Success()
        {
            var expected = JObject.Parse("{\"highlight\":{\"fields\":{\"*\":{\"type\":\"unified\"}}}}"); 

            var actual = new JObject().SetupHighlights();

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupHighlights_Null()
        {
            JObject stubValue = null;

            var actual = stubValue.SetupHighlights();

            actual.Should().BeNull();
        }
        [Fact]
        public void SetupHighlights_UpdateExistingProperty()
        {
            var expected = JObject.Parse("{\"highlight\":{\"fields\":{\"*\":{\"type\":\"unified\"}}}}"); 

            var stubValue = new JObject(
                new JProperty("highlight", "stub_value")
            );

            var actual = stubValue.SetupHighlights();

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName:fieldValue", true)]
        [InlineData("fieldName:fieldValue", false)]
        public void SetupExactQuery_Success(string query, bool lenient)
        {
            var expected = JObject.Parse("{\"query\":{\"query_string\":{\"query\":\""+query+"\", \"lenient\":"+lenient.ToString().ToLower()+"}}}");

            var actual = new JObject().SetupExactQuery(query, lenient);

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName:fieldValue")]
        public void SetupExactQuery_Success_NoLenient(string query)
        {
            var expected = JObject.Parse("{\"query\":{\"query_string\":{\"query\":\""+query+"\"}}}");

            var actual = new JObject().SetupExactQuery(query);

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupExactQuery_Null()
        {
            JObject stubValue = null;

            var actual = stubValue.SetupExactQuery("fieldName:fieldValue", true);

            actual.Should().BeNull();
        }
        [Fact]
        public void SetupExactQuery_UpdateExistingProperty()
        {
            var expected = JObject.Parse("{\"query\":{\"query_string\":{\"query\":\"fieldName:fieldValue\", \"lenient\":true}}}");

            var stubValue = new JObject(
                new JProperty("query", "query_stub_value")
            );

            var actual = stubValue.SetupExactQuery("fieldName:fieldValue", true);

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName", "asc")]
        [InlineData("fieldName", "desc")]
        public void SetupSorting_Success(string sortColumn, string sortOrder)
        {
            var expected = JObject.Parse("{\"sort\":[{\""+sortColumn+"\":{\"order\":\""+sortOrder+"\"}}]}");

            var actual = new JObject().SetupSorting(sortColumn, sortOrder);

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData(null, "asc")]
        [InlineData("fieldName", null)]
        public void SetupSorting_Null(string sortColumn, string sortOrder)
        {
            JObject stubValue = null;

            var actual = stubValue.SetupSorting(sortColumn, sortColumn);

            actual.Should().BeNull();
        }
        [Fact]
        public void SetupSorting_UpdateExistingProperty()
        {
            var expected = JObject.Parse("{\"sort\":[{\"fieldName1\":{\"order\":\"asc\"}}, {\"fieldName2\":{\"order\":\"desc\"}}]}");

            var stubValue = JObject.Parse("{\"sort\":[{\"fieldName1\":{\"order\":\"asc\"}}]}");

            var actual = stubValue.SetupSorting("fieldName2", "desc");

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupSorting_SortIsNotArray()
        {
            var expected = JObject.Parse("{\"sort\":{\"fieldName\":\"fieldValue\"}}");

            var stubValue = new JObject(
                new JProperty("sort", new JObject( new JProperty("fieldName", "fieldValue")))
            );

            var actual = stubValue.SetupSorting("fieldName", "fieldValue");

            actual.Should().BeEquivalentTo(expected);
        }
    }
}