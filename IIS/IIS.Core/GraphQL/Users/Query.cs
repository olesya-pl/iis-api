using System;
using HotChocolate;

namespace IIS.Core.GraphQL.Users
{
    public class Query
    {
        private static UserSession Stub => new UserSession
        {
            User = new User { Username = "User", Id = Guid.Empty, Name = "John Doe" },
            Token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJiOWFiODZiMC05ZjEwLTExZTktOGNjYy00ZGM4NDRjMWUzMjUiLCJpYXQiOjE1NjM4ODQ2MTMsImV4cCI6MTU2MzkyMDYxM30.7K3rP7jLZqTCgBnYYPKx3bbwc5udRgDZ1dhW7ONbLS4"
        };

        public UserSession GetUserSession([GraphQLNonNullType] string username, [GraphQLNonNullType] string password) => Stub;


        public UserSession RefreshToken() => Stub;
    }
}
