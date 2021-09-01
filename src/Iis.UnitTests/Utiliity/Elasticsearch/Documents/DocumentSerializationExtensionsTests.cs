using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Iis.Utility.Elasticsearch.Documents;

namespace Iis.UnitTests.Utiliity.Elasticsearch.Documents
{
    public class DocumentSerializationExtensionsTests
    {
        [Fact]
        public void ConvertToJson_WhenDocumentsIsNull_ShouldThrowArgumentNullException()
        {
            Dictionary<Guid, TestDocument> documents = null;

            Func<string> func = documents.ConvertToJson;

            func.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void ConvertToJson_WhenDocumentsIsEmpty_ShouldThrowArgumentException()
        {
            var documents = new Dictionary<Guid, TestDocument>();

            Func<string> func = documents.ConvertToJson;

            func.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [AutoMoqData]
        public void ConvertToJson_WhenSomeDocumentIsNull_ShouldThrowArgumentNullException(Guid id)
        {
            var documents = new Dictionary<Guid, TestDocument>
            {
                { id, null }
            };

            Func<string> func = documents.ConvertToJson;

            func.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void ConvertToJson_ShouldBeInCorrectFormat()
        {
            var documents = CreateTestData();
            string expectedResult = string.Join(string.Empty,
                "{\"index\":{\"_id\":\"0a641312abb74b40a7660781308eb077\"}}\n{\"Id\":\"0a641312-abb7-4b40-a766-0781308eb077\",\"Value\":\"Some value\"}",
                Environment.NewLine,
                "{\"index\":{\"_id\":\"0a641312abb74b40a7660781308eb088\"}}\n{\"Id\":\"0a641312-abb7-4b40-a766-0781308eb088\",\"Value\":\"Some value 1\"}",
                Environment.NewLine);

            string documentsJson = documents.ConvertToJson();

            documentsJson.Should().NotBeNullOrWhiteSpace();
            documentsJson.Should().BeEquivalentTo(expectedResult);
        }

        private static IReadOnlyDictionary<Guid, TestDocument> CreateTestData()
        {
            return new Dictionary<Guid, TestDocument>
            {
                {
                    new Guid("0a641312-abb7-4b40-a766-0781308eb077"),
                    new TestDocument
                    {
                        Id = new Guid("0a641312-abb7-4b40-a766-0781308eb077"),
                        Value = "Some value"
                    }
                },
                {
                    new Guid("0a641312-abb7-4b40-a766-0781308eb088"),
                    new TestDocument
                    {
                        Id = new Guid("0a641312-abb7-4b40-a766-0781308eb088"),
                        Value = "Some value 1"
                    }
                }
            };
        }

        private class TestDocument
        {
            public Guid Id { get; set; }
            public string Value { get; set; }
        }
    }
}