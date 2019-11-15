using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IIS.Core
{
    public static class TokenHelper
    {
        static string CLAIM_TYPE_UID = "uid";

        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
        static JwtSecurityTokenHandler _securityTokenHandler = new JwtSecurityTokenHandler();
        public static string NewToken(string issuer, string audiene, string securityKey, TimeSpan lifetime, IEnumerable<Claim> claims)
        {
            var now = DateTime.Now;
            var token = new JwtSecurityToken(
                    issuer,
                    null,
                    claims,
                    null,
                    now.Add(lifetime),
                    new SigningCredentials(GetSymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256));
            return _securityTokenHandler.WriteToken(token);
        }
        public static string NewToken(IConfiguration configuration, Guid userId)
        {
            return NewToken(
                configuration.GetValue<string>("jwt:issuer"),
                configuration.GetValue<string>("jwt:audience"),
                configuration.GetValue<string>("jwt:signingKey"),
                configuration.GetValue<TimeSpan>("jwt:lifeTime"),
                new Claim[] {
                    new Claim(CLAIM_TYPE_UID, userId.ToString())
                }
            );
        }

        public static JwtSecurityToken ValidateToken(string token, IConfiguration config)
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
                    ValidateAudience = false
                };

                _securityTokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return validatedToken as JwtSecurityToken;
            }
            catch (SecurityTokenException e)
            {
                throw new AuthenticationException("Token is invalid or has been expired");
            }
        }

        public static JwtSecurityToken ReadToken(string token) => _securityTokenHandler.ReadJwtToken(token);
    }
}
