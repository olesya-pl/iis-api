using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    public class MatrixLoginResponse
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("home_server")]
        public string HomeServer { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonIgnore]
        public string ErrorMessage { get; set; }
        [JsonIgnore]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    }
}
