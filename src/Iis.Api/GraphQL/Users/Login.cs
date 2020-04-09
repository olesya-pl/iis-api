using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HotChocolate.Resolvers;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Iis.Roles;

namespace IIS.Core.GraphQL.Users
{
    public class LoginResolver
    {
        private IConfiguration _configuration;
        private readonly OntologyContext _context;
        private IMapper _mapper;
        private RoleService _roleLoader;

        public LoginResolver(IConfiguration configuration, OntologyContext context, IMapper mapper, RoleService roleLoader)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
            _roleLoader = roleLoader;
        }

        public LoginResponse Login([Required] string username, [Required] string password)
        {
            var hash = _configuration.GetPasswordHashAsBase64String(password);
            var user = _roleLoader.GetUser(username, hash);

            if (user == null || user.IsBlocked)
                throw new InvalidCredentialException($"Wrong username or password");

            return new LoginResponse
            {
                User = _mapper.Map<User>(user),
                Token = TokenHelper.NewToken(_configuration, user.Id)
            };
        }

        public async Task<LoginResponse> RefreshToken(IResolverContext ctx) {
            var tokenPayload = ctx.ContextData["token"] as TokenPayload;

            if (tokenPayload == null)
                throw new NullReferenceException("Expected to have \"token\" in context's data.");

            // TODO: user should be found lazily only if client requests it
            var user = await _context.Users.FindAsync(tokenPayload.UserId);

            if (user == null)
                throw new InvalidCredentialException($"Invalid JWT token. Please re-login");

            return new LoginResponse
            {
                User = _mapper.Map<User>(user),
                Token = TokenHelper.NewToken(_configuration, Guid.NewGuid())
            };
        }
    }
}
