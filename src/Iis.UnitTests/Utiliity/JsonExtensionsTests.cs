using Iis.Utility;
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
    }
}
