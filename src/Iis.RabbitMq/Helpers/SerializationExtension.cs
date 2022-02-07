using System.Text;
using System.Text.Json;

namespace Iis.RabbitMq.Helpers
{
    internal static class SerializationExtension
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions => new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static byte[] ToByteArray<T>(this T value, JsonSerializerOptions options = null)
        {
            if (value is null) return default(byte[]);

            var json = JsonSerializer.Serialize<T>(value, options);

            return Encoding.UTF8.GetBytes(json);
        }

        public static T ToObject<T>(this byte[] value, JsonSerializerOptions options = null)
        {
            if (value is null || value.Length == 0) return default(T);

            var jsonString = Encoding.UTF8.GetString(value);

            return JsonSerializer.Deserialize<T>(jsonString, options);
        }
        public static string ToText(this byte[] value)
        {
            if (value is null || value.Length == 0) return null;

            return Encoding.UTF8.GetString(value);
        }
    }
}