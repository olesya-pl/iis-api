using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace IIS.Core
{
    public static class Hasher
    {
        public static string ComputeHash(string s)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = Encoding.UTF8.GetBytes(s);
                var generatedHash = sha1.ComputeHash(hash);
                var generatedHashString = Convert.ToBase64String(generatedHash);
                return generatedHashString;
            }
        }

        public static string GetPasswordHashAsBase64String(this IConfiguration configuration, string password)
        {
            var salt = configuration.GetValue<string>("salt", string.Empty);
            return ComputeHash(password + salt);
        }
    }
}
