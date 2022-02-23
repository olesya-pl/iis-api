using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate.Resolvers;
using System.Security.Authentication;
using System.Threading.Tasks;
using Iis.DataModel;
using AutoMapper;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Users
{
    public class LoginResolver
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IAuthTokenService _authTokenService;

        public LoginResolver(
            OntologyContext context,
            IMapper mapper,
            IUserService userService,
            IAuthTokenService authTokenService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
            _userService = userService;
            _authTokenService = authTokenService;
        }

        public async Task<LoginResponse> Login(
            IResolverContext context,
            [Required] string username,
            [Required] string password)
        {
            var user = await _userService.ValidateAndGetUserAsync(username, password, context.RequestAborted);

            return new LoginResponse
            {
                User = _mapper.Map<User>(user),
                Token = _authTokenService.NewToken(user.Id)
            };
        }

        public async Task<LoginResponse> RefreshToken(IResolverContext ctx)
        {
            var tokenPayload = ctx.GetToken();

            if (tokenPayload == null)
            {
                throw new NullReferenceException("Expected to have \"token\" in context's data.");
            }

            var user = await _context.Users.FindAsync(tokenPayload.UserId);

            if (user == null)
            {
                throw new InvalidCredentialException($"Invalid JWT token. Please re-login");
            }

            return new LoginResponse
            {
                User = _mapper.Map<User>(user),
                Token = _authTokenService.NewToken(Guid.NewGuid())
            };
        }
    }
}