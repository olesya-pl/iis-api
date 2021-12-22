using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Iis.Desktop.Common.GraphQL;

namespace Iis.Desktop.Common.Login
{
    public interface ILoginRequestWrapper
    {
        Task<GraphQLResponse<LoginResult>> Login(UserCredentials userCredentials);
    }
}
