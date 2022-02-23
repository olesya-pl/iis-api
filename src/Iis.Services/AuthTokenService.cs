using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.Users;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Iis.Services
{
    public class AuthTokenService : IAuthTokenService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _securityTokenHandler = new JwtSecurityTokenHandler();

        public AuthTokenService(
            IUserService userService,
            IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        public SymmetricSecurityKey GetSymmetricSecurityKey(string key) =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

        public string NewToken(Guid userId)
        {
            return NewToken(
                _configuration.GetValue<string>("jwt:issuer"),
                _configuration.GetValue<string>("jwt:audience"),
                _configuration.GetValue<string>("jwt:signingKey"),
                _configuration.GetValue<TimeSpan>("jwt:lifeTime"),
                new Claim[]
                {
                    new Claim(TokenPayload.ClaimTypeUid, userId.ToString())
                });
        }

        public async Task<TokenPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (!_securityTokenHandler.CanReadToken(token))
            {
                throw new AuthenticationException("Unable to read token");
            }

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _configuration.GetValue<string>("jwt:issuer"),
                    ValidAudience = _configuration.GetValue<string>("jwt:audience"),
                    IssuerSigningKey = GetSymmetricSecurityKey(_configuration.GetValue<string>("jwt:signingKey")),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };

                _securityTokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                var userId = TokenPayload.GetUserId(validatedToken as JwtSecurityToken);

                var user = await _userService.GetUserAsync(userId, cancellationToken);

                if (user is null)
                {
                    throw new SecurityTokenException();
                }

                return TokenPayload.From(validatedToken as JwtSecurityToken, user);
            }
            catch (SecurityTokenException)
            {
                throw new AuthenticationException("Token is invalid or has been expired");
            }
        }

        public async Task<TokenPayload> GetTokenPayloadAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.Headers.TryGetValue("Authorization", out var token))
            {
                throw new AuthenticationException("Requires \"Authorization\" header to contain a token");
            }

            return await ValidateTokenAsync(token, cancellationToken);
        }

        public async Task<User> GetHttpRequestUserAsync(HttpRequest request, CancellationToken cancellationToken = default) =>
            (await GetTokenPayloadAsync(request, cancellationToken))?.User;

        private string NewToken(string issuer, string audiene, string securityKey, TimeSpan lifetime, IEnumerable<Claim> claims)
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
    }
}
