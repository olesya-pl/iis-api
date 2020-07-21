using System;
using System.Security.Authentication;
using HotChocolate;

namespace Iis.Api.Bootstrap
{
    internal class AppErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception is InvalidOperationException || error.Exception is InvalidCredentialException)
            {

                return error.WithCode("BAD_REQUEST")
                    .WithMessage(error.Exception.Message);
            }

            if (error.Exception is AuthenticationException)
            {
                return error.WithCode("UNAUTHENTICATED");
            }

            if (error.Exception is AccessViolationException)
            {
                return error.WithCode("ACCESS_DENIED");
            }

            return error;
        }
    }
}