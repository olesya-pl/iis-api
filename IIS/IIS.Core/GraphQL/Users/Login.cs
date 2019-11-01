using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HotChocolate.Resolvers;
using System.Security.Authentication;
using Microsoft.AspNetCore.Http;

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
                Token = TokenHelper.NewToken(_configuration)
            };
        }

        public LoginResponse RefreshToken() {
            return new LoginResponse
            {
                User = null,
                Token = TokenHelper.NewToken(_configuration)
            };
        }
    }
}
