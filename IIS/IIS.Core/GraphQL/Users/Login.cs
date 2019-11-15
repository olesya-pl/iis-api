using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Execution;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace IIS.Core.GraphQL.Users
{
    public class LoginResolver
    {
        private IConfiguration _configuration;
        private readonly OntologyContext _context;

        public LoginResolver(IConfiguration configuration, OntologyContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public LoginResponse Login([Required] string username, [Required] string password)
        {
            var hash = _configuration.GetPasswordHashAsBase64String(password);
            var user = _context.Users.SingleOrDefault(u => u.Username.ToUpperInvariant() == username.ToUpperInvariant() && u.PasswordHash == hash);

            if (user == null || user.IsBlocked)
                throw new InvalidCredentialException($"Wrong username or password");

            return new LoginResponse
            {
                User = new User(user),
                Token = TokenHelper.NewToken(_configuration, user.Id)
            };
        }

        public async Task<LoginResponse> RefreshToken(IResolverContext ctx) {
            var token = ctx.ContextData["token"] as JwtSecurityToken;

            if (token == null)
                throw new NullReferenceException("Expected to have \"token\" in context's data.");

            // TODO: user should be found lazily only if client requests it
            var user = await _context.Users.FindAsync(Guid.Parse(token.Payload["uid"] as string));

            if (user == null)
                throw new InvalidCredentialException($"Invalid JWT token. Please re-login");

            return new LoginResponse
            {
                User = new User(user),
                Token = TokenHelper.NewToken(_configuration, Guid.NewGuid())
            };
        }
    }
}
