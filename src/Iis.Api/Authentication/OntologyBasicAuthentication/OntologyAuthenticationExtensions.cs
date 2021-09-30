using Microsoft.AspNetCore.Authentication;

namespace Iis.Api.Authentication.OntologyBasicAuthentication
{
    internal static class OntologyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddOntologyScheme(this AuthenticationBuilder builder)
        {
            builder.AddScheme<OntologyAuthenticationSchemeOptions, OntologyAuthenticationHandler>(AuthenticationSchemeConstants.OntologyAuthenticationScheme, _ => { });

            return builder;
        }
    }
}