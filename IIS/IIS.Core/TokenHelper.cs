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
        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
        static JwtSecurityTokenHandler _securityTokenHandler = new JwtSecurityTokenHandler();
        public static string NewToken(string issuer, string audiene, string securityKey, TimeSpan lifetime)
        {
            var now = DateTime.Now;
            var token = new JwtSecurityToken(
                    issuer,
                    audiene,
                    null,
                    now,
                    now.Add(lifetime),
                    new SigningCredentials(GetSymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256));
            return _securityTokenHandler.WriteToken(token);
        }
        public static string NewToken(IConfiguration configuration) => NewToken(configuration.GetValue<string>("jwt:issuer"), configuration.GetValue<string>("jwt:audience"), configuration.GetValue<string>("jwt:signingKey"), configuration.GetValue<TimeSpan>("jwt:lifeTime"));
        public static void ValidateToken(string token, TokenValidationParameters validationParameters)
        {
            if (!_securityTokenHandler.CanReadToken(token))
                throw new AuthenticationException("Unable to read token");

            try
            {
                _securityTokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            }
            catch (SecurityTokenException e)
            {
                throw new AuthenticationException(e.Message);
            }
        }

        public static JwtSecurityToken ReadToken(string token) => _securityTokenHandler.ReadJwtToken(token);
    }
}
