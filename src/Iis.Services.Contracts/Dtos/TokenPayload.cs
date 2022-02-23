using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Iis.Domain.Users;

namespace Iis.Services.Contracts.Dtos
{
    public class TokenPayload
    {
        public const string TokenPropertyName = "token";
        public const string ClaimTypeUid = "uid";
        public readonly Guid UserId;
        public User User;

        public static TokenPayload From(JwtSecurityToken token, User user)
        {
            return new TokenPayload(user.Id, user);
        }

        public static Guid GetUserId(JwtSecurityToken token)
        {
            var userId = Guid.Parse(token.Payload[ClaimTypeUid] as string);

            return userId;
        }

        public TokenPayload(Guid userId, User user)
        {
            UserId = userId;
            User = user;
        }
    }
}
