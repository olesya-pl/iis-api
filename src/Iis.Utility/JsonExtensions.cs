using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Utility
{
    public static class JsonExtensions
    {
        public static string ReplaceValue(this string json, string path, string newValue) 
        {
            var jObject = JObject.Parse(json);
            jObject.SelectToken(path, false).Replace(newValue);
            
            return jObject.ToString();
        }

        public static string ReplaceOrAddValues(this string json, params (string Path, string NewValue)[] changeItems)
        {
            var jObject = JObject.Parse(json);

            foreach (var item in changeItems)
            {
                var jToken = TryGetJToken(jObject, item.Path);
                if(jToken == null)
                {
                    jObject.Add(item.Path, item.NewValue);
                }
                else
                {
                    jToken.Replace(item.NewValue);
                }
            }

            return jObject.ToString();
        }

        private static JToken TryGetJToken(JObject jObject, string path) 
        {
            try
            {
                return jObject.SelectToken(path, false) ?? jObject.SelectToken($"['{path}']", false);
            }
            catch (JsonException)
            {
                return null;
            }
            
        }
    }
}
