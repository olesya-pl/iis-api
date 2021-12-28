using Newtonsoft.Json;

namespace Iis.Desktop.Common.Login
{
    public class LoginParam
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public override string ToString() => $"{Username} / {Password}";
    }
}
