using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Iis.Roles;
using System.Threading.Tasks;

namespace IIS.Core
{
    public static class TokenHelper
    {
        public static string CLAIM_TYPE_UID = "uid";
        public static string CLAIM_TYPE_PASSHASH = "passhash";

        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
        static JwtSecurityTokenHandler _securityTokenHandler = new JwtSecurityTokenHandler();
        public static string NewToken(string issuer, string audiene, string securityKey, TimeSpan lifetime, IEnumerable<Claim> claims)
        {
            DateTime expires = DateTime.Now.Add(lifetime);
            var token = new JwtSecurityToken(
                    issuer,
                    null,
                    claims,
                    null,
                    expires,
                    new SigningCredentials(GetSymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256));
            return _securityTokenHandler.WriteToken(token);
        }
        public static string NewToken(IConfiguration configuration, Guid userId, string userPasswordHash)
        {
            return NewToken(
                configuration.GetValue<string>("jwt:issuer"),
                configuration.GetValue<string>("jwt:audience"),
                configuration.GetValue<string>("jwt:signingKey"),
                configuration.GetValue<TimeSpan>("jwt:lifeTime"),
                new Claim[] {
                    new Claim(CLAIM_TYPE_UID, userId.ToString()),
                    new Claim(CLAIM_TYPE_PASSHASH, userPasswordHash)
                }
            );
        }

        public static TokenPayload ValidateToken(string token, IConfiguration config, UserService userService)
        {
            if (!_securityTokenHandler.CanReadToken(token))
                throw new AuthenticationException("Unable to read token");

            try
            {
                var validationParameters = new TokenValidationParameters {
                    ValidIssuer = config.GetValue<string>("jwt:issuer"),
                    ValidAudience = config.GetValue<string>("jwt:audience"),
                    IssuerSigningKey = GetSymmetricSecurityKey(config.GetValue<string>("jwt:signingKey")),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };

                _securityTokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                var userClaims = TokenPayload.GetUserClaims(validatedToken as JwtSecurityToken);

                var user = userService.GetUser(userClaims.userId, userClaims.userPasswordHash);

                if (user is null) throw new SecurityTokenException();

                return TokenPayload.From(validatedToken as JwtSecurityToken, user);
            }
            catch (SecurityTokenException)
            {
                throw new AuthenticationException("Token is invalid or has been expired");
            }
        }

        public static JwtSecurityToken ReadToken(string token) => _securityTokenHandler.ReadJwtToken(token);
    }

    public class TokenPayload
    {
        public readonly Guid UserId;
        public User User;

        public static TokenPayload From(JwtSecurityToken token, User user)
        {
            return new TokenPayload(user.Id, user);
        }

        public static (Guid userId, string userPasswordHash) GetUserClaims(JwtSecurityToken token) 
        {
            var userId = Guid.Parse(token.Payload[TokenHelper.CLAIM_TYPE_UID] as string);
            var userHash = token.Payload[TokenHelper.CLAIM_TYPE_PASSHASH] as string;

            return (userId, userHash);
        }
        public TokenPayload(Guid userId, User user)
        {
            UserId = userId;
            User = user;
        }
    }
}
