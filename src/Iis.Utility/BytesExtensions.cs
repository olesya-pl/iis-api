using System.Text;
using Newtonsoft.Json;

namespace Iis.Utility
{
    public static class BytesExtensions
    {
        public static byte[] ToBytes(this object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));

        public static T FromBytes<T>(this byte[] bytes) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
    }
}