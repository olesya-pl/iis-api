using Iis.Domain.Users;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Iis.Api.Authentication.OntologyBasicAuthentication
{
    internal class OntologyAuthenticationHandler : AuthenticationHandler<OntologyAuthenticationSchemeOptions>
    {
        private const int HeaderParametersCount = 2;
        private const char HeaderParameterSeparator = ':';

        private readonly IUserService _userService;

        public OntologyAuthenticationHandler(
            IOptionsMonitor<OntologyAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>();
            if (allowAnonymous != null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                return AuthenticateResult.Fail($"Missing '{HeaderNames.Authorization}' header");
            }

            if (!TryObtainCredentials(out var result))
            {
                return AuthenticateResult.Fail($"Invalid '{HeaderNames.Authorization}' Header");
            }

            User user;

            try
            {
                user = await _userService.ValidateAndGetUserAsync(result.Username, result.Password, Context.RequestAborted);
            }
            catch (InvalidCredentialException ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }

            if (!IsAuthorized(user))
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }

            var ticket = GenerateTicket(user);

            return AuthenticateResult.Success(ticket);
        }

        private AuthenticationTicket GenerateTicket(User user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationTicket(principal, Scheme.Name);
        }

        private bool TryObtainCredentials(out (string Username, string Password) result)
        {
            try
            {
                var headerDictionary = Request.Headers[HeaderNames.Authorization];
                var authHeader = AuthenticationHeaderValue.Parse(headerDictionary);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8
                    .GetString(credentialBytes)
                    .Split(HeaderParameterSeparator, HeaderParametersCount);

                result = (credentials[0], credentials[1]);
                return true;
            }
            catch
            {
                result = (null, null);
                return false;
            }
        }

        private bool IsAuthorized(User user) => user != null && user.IsAdmin;
    }
}