using FluentAssertions;
using Iis.Utility;
using Quibble.Xunit;
using Xunit;

namespace Iis.UnitTests.Utility
{
    public class JsonExtensionsTests
    {
        [Fact]
        public void ReplaceValue_ShouldCorrectReplaceName() 
        {
            //Arrange
            var json = "{ data: {user: { name: \"Pavlo\" } }}";
            
            //Act
            var updatedJson = json.ReplaceValue("data.user.name", "Vova");

            //Assert
            Assert.Contains("Vova", updatedJson);
        }

        [Fact]
        public void ReplaceOrAddValue_ShouldReplaceValue()
        {
            //Arrange
            var json = "{ \"data\": {\"user\": { \"name\": \"Pavlo\" } }}";

            //Act
            var updatedJson = json.ReplaceOrAddValues(("data.user.name", "Vova"));

            //Assert
            JsonAssert.Equal("{ \"data\": {\"user\": { \"name\": \"Vova\" } }}", updatedJson);
        }

        [Fact]
        public void ReplaceOrAddValue_ShouldHandleEmptyPathGracefully()
        {
            //Arrange
            var json = "{ \"data\": {\"user\": { \"name\": \"Pavlo\" } }}";

            //Act
            var updatedJson = json.ReplaceOrAddValues(("", "Vova"));

            //Assert
            JsonAssert.Equal("{ \"data\": {\"user\": { \"name\": \"Pavlo\" } }}", updatedJson);
        }
    }
}
