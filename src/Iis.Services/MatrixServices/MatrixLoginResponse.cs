using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    public class MatrixLoginResponse
    {
        public string user_id { get; set; }
        public string access_token { get; set; }
        public string home_server { get; set; }
        public string device_id { get; set; }
    }
}
