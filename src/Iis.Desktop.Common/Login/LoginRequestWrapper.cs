using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Iis.Desktop.Common.GraphQL;

namespace Iis.Desktop.Common.Login
{
    public class LoginRequestWrapper : ILoginRequestWrapper
    {

        private GraphQLRequestWrapper<LoginParam, LoginResult> _graphQLRequestWrapper;

        public LoginRequestWrapper(Uri uri)
        {
            _graphQLRequestWrapper = new GraphQLRequestWrapper<LoginParam, LoginResult>(uri);
        }

        public async Task<GraphQLResponse<LoginResult>> LoginAsync(UserCredentials userCredentials)
        {
            var operationName = "login";
            var query = "mutation login($username:String, $password:String) { login(username: $username, password: $password) { token } }";
            var param = new LoginParam
            {
                Username = userCredentials.UserName,
                Password = userCredentials.Password
            };
            var result = await _graphQLRequestWrapper.SendAsync(query, param, operationName);
            return result;
        }
    }
}
