using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.MatrixServices
{
    internal class MatrixLoginRequest
    {
        public string type { get; set; } = "m.login.password";
        public string user { get; set; }
        public string password { get; set; }
    }
}
