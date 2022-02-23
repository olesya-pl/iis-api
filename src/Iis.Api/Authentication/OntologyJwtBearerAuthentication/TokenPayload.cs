using System;
using Iis.Domain.Users;

namespace Iis.Api.Authentication.OntologyJwtBearerAuthentication
{
    public class TokenPayload
    {
        public const string TokenPropertyName = "token";

        public TokenPayload(User user)
        {
            UserId = user.Id;
            User = user;
        }

        public Guid UserId { get; }
        public User User { get; }
    }
}