using System;
using System.Text;
using System.Text.Json;

namespace Iis.RabbitMq.Helpers
{
    internal static class SerializationExtension
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions => new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static ReadOnlyMemory<byte> ToByteArray<T>(this T value, JsonSerializerOptions options = null)
        {
            if (value is null) return default(byte[]);

            var json = JsonSerializer.Serialize<T>(value, options);

            return new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
        }

        public static T ToObject<T>(this ReadOnlyMemory<byte> value, JsonSerializerOptions options = null)
        {
            if (value.Length == 0) return default(T);

            var jsonString = Encoding.UTF8.GetString(value.Span);

            return JsonSerializer.Deserialize<T>(jsonString, options);
        }
        public static string ToText(this ReadOnlyMemory<byte> value)
        {
            if (value.Length == 0) return null;

            return Encoding.UTF8.GetString(value.Span);
        }
    }
}