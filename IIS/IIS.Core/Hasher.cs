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
    }
}
