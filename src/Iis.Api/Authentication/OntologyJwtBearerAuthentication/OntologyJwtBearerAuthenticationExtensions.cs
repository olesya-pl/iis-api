using System;
using IIS.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Iis.Api.Authentication.OntologyJwtBearerAuthentication
{
    public static class OntologyJwtBearerAuthenticationExtensions
    {
        public static AuthenticationBuilder AddOntologyJwtBearerAuthentication(
            this AuthenticationBuilder authenticationBuilder,
            IConfiguration configuration)
        {
            authenticationBuilder.AddJwtBearer(_ =>
            {
#if DEBUG
                IdentityModelEventSource.ShowPII = true;
#endif
                _.RequireHttpsMetadata = false;
                _.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration.GetValue<string>("jwt:issuer"),
                    ValidAudience = configuration.GetValue<string>("jwt:audience"),
                    IssuerSigningKey = TokenHelper.GetSymmetricSecurityKey(configuration.GetValue<string>("jwt:signingKey")),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return authenticationBuilder;
        }
    }
}