using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Desktop.Common.Login
{
    public class UserCredentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public override string ToString() => $"{UserName} / {Password}";
    }
}
