using System;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.Users;
using Iis.Services.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAuthTokenService
    {
        SymmetricSecurityKey GetSymmetricSecurityKey(string key);
        string NewToken(Guid userId);
        Task<TokenPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<TokenPayload> GetTokenPayloadAsync(HttpRequest request, CancellationToken cancellationToken = default);
        Task<User> GetHttpRequestUserAsync(HttpRequest request, CancellationToken cancellationToken = default);
    }
}