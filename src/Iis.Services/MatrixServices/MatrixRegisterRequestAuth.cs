using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    public class MatrixRegisterRequestAuth
    {
        [JsonProperty("type")]
        public string AuthType { get; set; } = "m.login.dummy";
    }
}
