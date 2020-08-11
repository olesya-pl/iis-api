using Iis.Services;
using Xunit;

namespace Iis.UnitTests.Services
{
    public class SanitizeServiceTests
    {

        [Fact]
        public void SanitizeBody_RequestLoginBody_PasswordAndLoginShouldBeReplaced()
        {
            //Arrange
            var requestBody =
                "{\"operationName\":\"loginOnApi\",\"variables\":{\"username\":\"userlogin\",\"password\":\"userpassword\"},\"query\":\"mutation loginOnApi($username:String!,$password:String!){login(username:$username,password:$password){tokenuser{iduserNamefullNameisAdminentities{kindtitleallowedOperations__typename}tabs{kindtitlevisible__typename}__typename}__typename}}\"}";

            var service = new SanitizeService();

            //Act
            var result = service.SanitizeBody(requestBody);

            //Assert
            Assert.DoesNotContain("userlogin", result);
            Assert.DoesNotContain("userpassword", result);
        }

        [Fact]
        public void SanitizeBody_E2ERequestLoginBody_PasswordAndLoginShouldBeReplaced()
        {
            //Arrange
            var requestBody =
                "{\"operationName\":\"loginOnApi\",\"variables\":{\"username\":\"userlogin\",\"password\":\"userpassword\"},\"query\":\"mutation loginOnApi($username: String!, $password: String!) {\\n  login(username: $username, password: $password) {\\n    token\\n    user {\\n      id\\n      userName\\n      fullName\\n      isAdmin\\n      entities {\\n        kind\\n        title\\n        allowedOperations\\n        __typename\\n      }\\n      tabs {\\n        kind\\n        title\\n        visible\\n        __typename\\n      }\\n      __typename\\n    }\\n    __typename\\n  }\\n}\\n\"}";

            var service = new SanitizeService();

            //Act
            var result = service.SanitizeBody(requestBody);

            //Assert
            Assert.DoesNotContain("userlogin", result);
            Assert.DoesNotContain("userpassword", result);
        }

        [Fact]
        public void SanitizeBody_NotValidBody_TheSameBodyShouldBeReturned()
        {
            //Arrange
            var service = new SanitizeService();

            //Act
            var result = service.SanitizeBody("some random text");

            //Assert
            Assert.Equal("some random text", result);
        }

        [Fact]
        public void SanitizeBody_ResponseLoginBody_TokenShouldBeReplaced()
        {
            //Arrange
            var responseBody = "{\"data\":{\"login\":{\"token\":\"eyJhbGciOiJIUzI1NiIsasdakjsdalksd.eyJ1aWQiOiJkODZmODk2MS04MTdiLTQ0OTQtYjBhOC0zMzBjMDM3YjUwqwkejaksdjaklsdjaklJ9.ElPDUps6frPCgtxoZuFqXT3VyBvPjagw3qNXy7EPa0c\"}}";
            var service = new SanitizeService();

            //Act
            var result = service.SanitizeBody(responseBody);

            //Assert
            Assert.DoesNotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", result);
        }
    }
}
