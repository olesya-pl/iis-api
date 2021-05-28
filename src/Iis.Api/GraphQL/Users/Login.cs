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
using Iis.Services;
using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Interfaces;
using Iis.Interfaces.Users;

namespace IIS.Core.GraphQL.Users
{
    public class LoginResolver
    {
        private readonly OntologyContext _context;
        private IMapper _mapper;
        private IUserService _userService;
        private IExternalUserService _externalUserService;
        private IConfiguration _configuration;

        public LoginResolver(
            IConfiguration configuration, 
            OntologyContext context, 
            IMapper mapper, 
            IUserService userService,
            IExternalUserService externalUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
            _userService = userService;
            _externalUserService = externalUserService;
            _configuration = configuration;
        }

        public LoginResponse Login([Required] string username, [Required] string password)
        {
            var user = _userService.ValidateAndGetUser(username, password);

            return new LoginResponse
            {
                User = _mapper.Map<User>(user),
                Token = TokenHelper.NewToken(_configuration, user.Id)
            };
        }

        public async Task<LoginResponse> RefreshToken(IResolverContext ctx) {
            var tokenPayload = ctx.GetToken();

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
