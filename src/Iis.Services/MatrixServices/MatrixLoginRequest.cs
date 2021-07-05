using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    internal class MatrixLoginRequest
    {
        [JsonProperty("type")]
        public string AuthType { get; set; } = "m.login.password";
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("identifier")]
        public MatrixLoginRequestIdentifier Identifier { get; set; }

        public MatrixLoginRequest(string userName, string password)
        {
            Identifier = new MatrixLoginRequestIdentifier { UserName = userName };
            Password = password;
        }
    }
}
