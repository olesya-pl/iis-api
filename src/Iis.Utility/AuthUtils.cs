using System;
using System.Text;

namespace Iis.Utility
{
    public static class AuthUtils
    {
        public static string GenerateBasicAuthHeaderValue(string login, string password)
        {
            var bytes = Encoding.UTF8.GetBytes($"{login}:{password}");
            var base64 = Convert.ToBase64String(bytes);
            return $"Basic {base64}";
        }
    }
}
