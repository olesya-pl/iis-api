using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Desktop.Common.Login
{
    public class LoginResult
    {
        public string Token { get; set; }
        public override string ToString() => Token;
    }
}
