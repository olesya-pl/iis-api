using System.Linq;
using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Iis.DbLayer.Repositories.Helpers;

namespace Iis.UnitTests.Iis.DbLayer.Repositories.Helpers
{
    public class FaceAPIResponseParserTests
    {
        [Fact]
        public void GetFaceVector_WithErrorContentReturnsNull()
        {
            var content = "{\"помилка\": \"Response status code does not indicate success: 500 (Internal Server Error).\"}";

            var imageVector = FaceAPIResponseParser.GetFaceVector(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetFaceVector_WithArrayAsContentReturnsArray()
        {
            var content = "[  -0.36,  0.16,  0.16,  0.12]";

            var expected = new decimal[] { -0.36m, 0.16m, 0.16m, 0.12m };

            var imageVector = FaceAPIResponseParser.GetFaceVector(content);

            imageVector.Should().Equal(expected);
        }
        [Fact]
        public void GetFaceVector_WithObjectAsContentReturnsArray()
        {
            var content = "{ \"time\": 179, \"result\": [ { \"location\": [ [ 187, 96 ], [ 262, 171 ] ], \"encoding\": [ -0.086, 0.069, 0.063, -0.024 ] } ] }";

            var expected = new decimal[] { -0.086m, 0.069m, 0.063m, -0.024m };

            var imageVector = FaceAPIResponseParser.GetFaceVector(content);

            imageVector.Should().Equal(expected);
        }
        [Fact]
        public void GetFaceVector_NoResultPropertyInContentReturnsNull()
        {
            var content = "{ \"time\": 179 }";

            var imageVector = FaceAPIResponseParser.GetFaceVector(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetFaceVector_EmptyResultPropertyInContentReturnsNull()
        {
            var content = "{ \"time\": 179, \"result\": [] }";

            var imageVector = FaceAPIResponseParser.GetFaceVector(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetFaceVector_EmptyEncodingArrayInContentReturnsNull()
        {
            var content = "{ \"time\": 179, \"result\": [ { \"location\": [ [ 187, 96 ], [ 262, 171 ] ], \"encoding\": [] } ] }";

            var imageVector = FaceAPIResponseParser.GetFaceVector(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetFaceVector_ResultWithTwoItemsInContentReturnsFirst()
        {
            var actualContent = "{ 'time': 300, 'result': [ { 'location': [ [ 165, 65 ], [ 208, 108 ] ], 'encoding': [ -0.08116906136274338, -0.027682635933160782, 0.06678098440170288 ] }, { 'location': [ [ 233, 61 ], [ 285, 113 ] ], 'encoding': [ -0.014624187722802162, 0.0325000174343586, 0.04493800178170204 ] } ] }";

            var expected = new decimal[]{-0.08116906136274338M,-0.027682635933160782M,0.06678098440170288M};

            FaceAPIResponseParser.GetFaceVector(actualContent)
                .Should()
                .NotBeNull()
                .And.HaveCount(expected.Length)
                .And.BeEquivalentTo(expected);
        }
        [Fact]
        public void GetFaceVectorList_EmptyContentReturnsEmptyList()
        {
            FaceAPIResponseParser.GetFaceVectorList(string.Empty)
                .Should()
                .BeEmpty();
        }
        [Fact]
        public void GetFaceVectorList_ContentWithErrorMessageReturnsEmptyList()
        {
            var actualContent = "{'помилка': 'Response status code does not indicate success: 500 (Internal Server Error).'}";

            FaceAPIResponseParser.GetFaceVectorList(actualContent)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetFaceVectorList_ContentWithArrayReturnsListWithArray()
        {
            var actualContent = "[  -0.36,  0.16,  0.16,  0.12]";

            var expected = new decimal[][] { new decimal[] { -0.36m, 0.16m, 0.16m, 0.12m } };

            FaceAPIResponseParser.GetFaceVectorList(actualContent)
                .Should()
                .NotBeEmpty()
                .And.HaveCount(expected.Length)
                .And.BeEquivalentTo(expected);
        }

        [Fact]
        public void GetFaceVectorList_ContentWith3ItemsReturnsListWith3Vectors()
        {
            var actualContent = "{ 'time': 300, 'result': [ { 'location': [ [ 165, 65 ], [ 208, 108 ] ], 'encoding': [ -0.08116906136274338, -0.027682635933160782, 0.06678098440170288 ] }, { 'location': [ [ 233, 61 ], [ 285, 113 ] ], 'encoding': [ -0.014624187722802162, 0.0325000174343586, 0.04493800178170204 ] }, { 'location': [ [ 31, 74 ], [ 74, 118 ] ], 'encoding': [ -0.04254836216568947, -0.020189080387353897, 0.05614340677857399 ] } ] }";

            var expected = new decimal[][]
            {
                new decimal[]{-0.08116906136274338M,-0.027682635933160782M,0.06678098440170288M},
                new decimal[]{-0.014624187722802162M,0.0325000174343586M,0.04493800178170204M},
                new decimal[]{-0.04254836216568947M,-0.020189080387353897M,0.05614340677857399M}
            };

            FaceAPIResponseParser.GetFaceVectorList(actualContent)
                .Should()
                .NotBeEmpty()
                .And.HaveCount(expected.Length)
                .And.BeEquivalentTo(expected);
        }

        [Fact]
        public void GetFaceVectorList_ContentWithEmptyResultPropertyReturnsEmptyList()
        {
            var actualContent = "{ 'time': 300, 'result': [ ] }";

            FaceAPIResponseParser.GetFaceVectorList(actualContent)
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetFaceVectorList_ContentWith1EmptyAnd2NormalItemsReturnsListWith2Vectors()
        {
            var actualContent = "{ 'time': 300, 'result': [ { 'location': [ [ 165, 65 ], [ 208, 108 ] ], 'encoding': [] }, { 'location': [ [ 233, 61 ], [ 285, 113 ] ], 'encoding': [ -0.014624187722802162, 0.0325000174343586, 0.04493800178170204 ] }, { 'location': [ [ 31, 74 ], [ 74, 118 ] ], 'encoding': [ -0.04254836216568947, -0.020189080387353897, 0.05614340677857399 ] } ] }";

            var expected = new decimal[][]
            {
                new decimal[]{-0.014624187722802162M,0.0325000174343586M,0.04493800178170204M},
                new decimal[]{-0.04254836216568947M,-0.020189080387353897M,0.05614340677857399M}
            };

            FaceAPIResponseParser.GetFaceVectorList(actualContent)
                .Should()
                .NotBeEmpty()
                .And.HaveCount(expected.Length)
                .And.BeEquivalentTo(expected);
        }

        [Fact]
        public void GetFaceVectorList_EmptyEncodingArrayInContentReturnsEmptyList()
        {
            var actualContent = "{ \"time\": 179, \"result\": [ { \"location\": [ [ 187, 96 ], [ 262, 171 ] ], \"encoding\": [] } ] }";

            FaceAPIResponseParser.GetFaceVectorList(actualContent)
                .Should()
                .BeEmpty();
        }
    }
}