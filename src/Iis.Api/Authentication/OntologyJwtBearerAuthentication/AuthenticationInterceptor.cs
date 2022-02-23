using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Api.Authentication.OntologyJwtBearerAuthentication
{
    public class AuthenticationInterceptor : DefaultHttpRequestInterceptor
    {
        public override async ValueTask OnCreateAsync(
            HttpContext context,
            IRequestExecutor requestExecutor,
            IQueryRequestBuilder requestBuilder,
            CancellationToken cancellationToken)
        {
            var userService = context.RequestServices.GetRequiredService<IUserService>();
            var userIdString = context.User.FindFirstValue(AuthenticationSchemeConstants.ClaimTypeUID);

            if (userIdString != null
                && Guid.TryParse(userIdString, out var userId))
            {
                var user = await userService.GetUserAsync(userId, context.RequestAborted);
                var tokenPayload = new TokenPayload(user);

                requestBuilder.TryAddProperty(TokenPayload.TokenPropertyName, tokenPayload);
            }

            await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
        }
    }
}