using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iis.Desktop.Common.GraphQL;

namespace Iis.Desktop.Common.Login
{
    public class LoginRequestWrapper : ILoginRequestWrapper
    {

        private GraphQLRequestWrapper<LoginParam, LoginResult> _graphQLRequestWrapper;
        
        private const string AttributesManagerTab = "AttributesManagerTab";
        
        private static readonly GraphQLResponse<LoginResult> NoPermissionResponse = new GraphQLResponse<LoginResult>(null, new GraphQLError
        {
            Message = "User doesn`t have a permission."
        });

        public LoginRequestWrapper(Uri uri)
        {
            _graphQLRequestWrapper = new GraphQLRequestWrapper<LoginParam, LoginResult>(uri);
        }

        public async Task<GraphQLResponse<LoginResult>> LoginAsync(UserCredentials userCredentials)
        {
            var operationName = "login";
            var query = "mutation login($username:String, $password:String) { login(username: $username, password: $password) { token user { tabs { kind, visible } isAdmin } } }";
            var param = new LoginParam
            {
                Username = userCredentials.UserName,
                Password = userCredentials.Password
            };
            var response = await _graphQLRequestWrapper.SendAsync(query, param, operationName);

            if (!response.IsSuccess) return response;
            
            return HasPermissionToAttributesManager(response) ? response : NoPermissionResponse;
        }

        private static bool HasPermissionToAttributesManager(GraphQLResponse<LoginResult> response)
        {
            return response.Result.User.IsAdmin || response.Result.User.Tabs.Any(tab => tab.Kind == AttributesManagerTab && tab.Visible);
        }
    }
}
