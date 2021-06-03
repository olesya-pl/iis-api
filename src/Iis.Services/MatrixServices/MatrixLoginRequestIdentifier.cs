using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    public class MatrixLoginRequestIdentifier
    {
        [JsonProperty("type")]
        public string IdentifierType { get; set; } = "m.id.user";
        [JsonProperty("user")]
        public string UserName { get; set; }
    }
}
