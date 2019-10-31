using HotChocolate;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public LoginResponse Login([Required] string login, [Required] string password)
        {
            var hash = _configuration.GetPasswordHashAsBase64String(password);
            var user = _context.Users.SingleOrDefault(u => u.UserName.ToUpperInvariant() == login.ToUpperInvariant() && u.PasswordHash == hash);

            if (user == null)
                throw new InvalidOperationException($"User with {login} and specified password not found");

            if (user.IsBlocked)
                throw new InvalidOperationException($"Account {login} blocked");

            return new LoginResponse
            {
                CurrentUser = new User(user),
                Token = TokenHelper.NewToken(_configuration)
            };
        }

        public string RefreshToken() => TokenHelper.NewToken(_configuration);
    }
}
