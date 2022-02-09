using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Desktop.Common.Login
{
    public class LoginResult
    {
        public string Token { get; set; }
        public User User { get; set; }
        public override string ToString() => Token;
    }

    public class User
    {
        public bool IsAdmin { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
    
    public class AccessTab
    {
        public Guid Id { get; set; }

        public string Kind { get; set; }

        public bool Visible { get; set; }
    }
}
