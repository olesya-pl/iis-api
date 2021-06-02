using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    public class MatrixRegisterRequest
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("auth")]
        public MatrixRegisterRequestAuth Auth { get; set; } = new MatrixRegisterRequestAuth();
    }
}
