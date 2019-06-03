using System.Linq;

namespace IIS
{
    public static class StringExtensions
    {
        public static string Camelize(this string value) => value.Substring(0, 1).ToUpper() + value.Substring(1);

        public static string Camelize(this ScalarType value)
        {
            var stringValue = (string)value;
            return stringValue.Substring(0, 1).ToUpper() + stringValue.Substring(1);
        }

        public static string ToLowerCamelcase(this string value) => value.Substring(0, 1).ToLower() + value.Substring(1);

        public static string ToUnderscore(this string str) =>
            string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
}
