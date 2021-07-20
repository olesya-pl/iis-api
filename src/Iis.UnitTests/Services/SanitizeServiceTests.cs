using System;
using FluentAssertions;
using Iis.Utility.Logging;
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

        [Fact]
        public void SanitizeBody_RequestBody_should_not_contain_base64_image()
        {
            //Arrange
            var responseBody = CreateString(3000);
            var service = new SanitizeService();

            //Act
            var result = service.SanitizeBody(responseBody);

            //Assert
            result.Length.Should().Be(2000 + " (...)Trimmed".Length);
        }

        private static string CreateString(int stringLength)
        {
            Random rd = new Random();
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        [Fact]
        public void SanitizeBody_RequestBody_should_not_exceed_2k_symbols_and_trim_message()
        {
            //Arrange
            var responseBody = "{\"operationName\":\"getMaterials\",\"variables\":{\"pagination\":{\"page\":1,\"pageSize\":50},\"filter\":{\"suggestion\":\"\"},\"sorting\":null,\"searchByImageInput\":{\"content\":\"iVBORw0KGgoAAAANSUhEUgAABEcAAARKCAYAAACtuB5QAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAJOgAACToAYJjBRwAAP+lSURBVHhe7P2FsiRbliUIOrtfv87sftHpMjPaZWZmZmYmZ2Z+jPECHkXEi4iMjEiorKzKqq7M6u6akQaRkukRGZGS+Yw1a2018/ciq+cLyk1kyVFTUzLVczYs3XufbXj3efd593n3efd593n3efd593n3efd593n3efd593n3+R/4s+2//bf/hnd4h3d4h3d4h3d4h3d4h3d4h3d4h3d4h/9R8Y4ceYd3eId3eId3eId3eId3eId3eId3eIf/oWFpNfnhO1EUuQcFEbu5vJvL+wgvFITtQUmEF4rDvdh6ozTyEMqiDqEk8gC/70VZ9B5UxOwl9qEqdj8qY/ajOvYA6hIOoz7hKOrjj6Ah4Qiak46hMfEo6uIOoCHpMJpSj6Au6QCqE/ahPpXrMg+iPoP7pXuj0cXtXUfR4jqO9qzTaHOdQlf2GfTn+aIn5xxaM06ghWhOP4EmopGoTz+O2tRjqEvj94xTaCIa0k6iIfU46pKPooLXVha9D6VEcfReFEbyf0bsQn7kLhRE7UZ22A64QrYjJ3QX8kJ2IefSNrjOb0PGhW1IY5tKpF3YjqSAbUg5vx2pF3ciOXA7l3cgyX8bsR3JfjuRErAHSWzjfbRuB1L8uZ0ft/HdjsRzXEek8rd0XweZ3NfFY2Zf2IHcSzuJHci5uJ3tNmI78oL5/TKv6/I2Qz6vsTSK/yPKC0Xhe6yt5X2tSTzCe3mY//cE+ovPY6jsEnERk3VhmG2KxFJ7LBaaozDXGI6Z+hBM1l7EaGUABkrOoqfwJLrzT6Az7zhaee87c06iLYv3Nu0I22NozTrEe+qF9tzD6C0+ib6SU+gvP4Px+gBMNp3HbMtFrHSHYb0vEqs94djsj8ZaTyzWe+Kx0ZuIzb4UbPakYq0jGasdqVjvzMBGdxaRjfXeHMMy1610pmG9Ox1rQg+/c5/FriTMd8Zjitc/1RmHgYYwtJQGoJLXV5h6ECUZR5CX5IWs2F3IjtsNV9QOZEbw/sXuQV7sPuTHeRMHURB/GMUpJ1Ge6YMKly9K2RZnnkN+2hmkRB9G5KW9CL2wF0EBe3Hh3G74HN+OM4e34dzRHfA5thPnju2A34kduHR2F0K4TWSQN5JjTiAp+ihiQvcjmogI9kLIpX24FLgX5333ItBnHwJ89iPQ/yBxGAH+B+Dnux8+5==\",\"name\":\"немцев.png\"}},\"query\":\"query getMaterials($pagination: PaginationInput!, $filter: FilterInput, $sorting: SortingInput, $searchByImageInput: SearchByImageInput) {\\n  materials(pagination: $pagination, filter: $filter, sorting: $sorting, searchByImageInput: $searchByImageInput) {\\n    count\\n    items {\\n      ...MaterialInfo\\n      highlight\\n      createdDate\\n      from\\n      mlHandlersCount\\n      processedMlHandlersCount\\n      objectsOfStudy\\n      processedStatus {\\n        id\\n        __typename\\n      }\\n      sessionPriority {\\n        ...MaterialSignInfo\\n        __typename\\n      }\\n      importance {\\n        ...MaterialSignInfo\\n        __typename\\n      }\\n      __typename\\n    }\\n    __typename\\n  }\\n}\\n\\nfragment MaterialInfo on Material {\\n  id\\n  data {\\n    type\\n    text\\n    __typename\\n  }\\n  title\\n  file {\\n    contentType\\n    id\\n    name\\n    url\\n    __typename\\n  }\\n  transcriptions\\n  coordinates\\n  content\\n  metadata\\n  __typename\\n}\\n\\nfragment MaterialSignInfo on MaterialSign {\\n  id\\n  shortTitle\\n  title\\n  __typename\\n}\\n\"}";
            var service = new SanitizeService();

            //Act
            var result = service.SanitizeBody(responseBody);

            //Assert
            result.Should().BeEquivalentTo("{\"operationName\":\"getMaterials\",\"variables\":{\"pagination\":{\"page\":1,\"pageSize\":50},\"filter\":{\"suggestion\":\"\"},\"sorting\":null,\"searchByImageInput\":{\"content\":\"base64image\",\"name\":\"немцев.png\"}},\"query\":\"query getMaterials($pagination: PaginationInput!, $filter: FilterInput, $sorting: SortingInput, $searchByImageInput: SearchByImageInput) {\\n  materials(pagination: $pagination, filter: $filter, sorting: $sorting, searchByImageInput: $searchByImageInput) {\\n    count\\n    items {\\n      ...MaterialInfo\\n      highlight\\n      createdDate\\n      from\\n      mlHandlersCount\\n      processedMlHandlersCount\\n      objectsOfStudy\\n      processedStatus {\\n        id\\n        __typename\\n      }\\n      sessionPriority {\\n        ...MaterialSignInfo\\n        __typename\\n      }\\n      importance {\\n        ...MaterialSignInfo\\n        __typename\\n      }\\n      __typename\\n    }\\n    __typename\\n  }\\n}\\n\\nfragment MaterialInfo on Material {\\n  id\\n  data {\\n    type\\n    text\\n    __typename\\n  }\\n  title\\n  file {\\n    contentType\\n    id\\n    name\\n    url\\n    __typename\\n  }\\n  transcriptions\\n  coordinates\\n  content\\n  metadata\\n  __typename\\n}\\n\\nfragment MaterialSignInfo on MaterialSign {\\n  id\\n  shortTitle\\n  title\\n  __typename\\n}\\n\"}");
        }
    }
}
