using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Iis.Api.Common.Middleware
{
    internal class ValidateAuthenticationMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                await next(context);
                return;
            }

            await context.ChallengeAsync();
        }
    }
}