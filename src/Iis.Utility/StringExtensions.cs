using Humanizer;
using System.Linq;
using System.Globalization;
using System.Text;

namespace Iis.Utility
{
    public static class StringExtensions
    {
        public static string Camelize(this string value) => value.Substring(0, 1).ToUpper() + value.Substring(1);

        public static string ToLowerCamelcase(this string value) => value.Substring(0, 1).ToLower() + value.Substring(1);

        public static string ToUnderscore(this string str) => str.Underscore();
        
        public static string RemoveWhiteSpace(this string input)
        {
            if(string.IsNullOrWhiteSpace(input)) return input;

            return new string(input.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
        }

        public static string RemoveNewLinesCharacter(this string input)
        {
            var removeChars = new char[] { '\n', '\r', '\t' };
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (!removeChars.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string ToLowerCamelCase(this string input)
        {
            if(string.IsNullOrWhiteSpace(input)) return input;

            input = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input);

            var charArray = input.ToCharArray();

            charArray[0] = char.ToLowerInvariant(charArray[0]);

            return new string(charArray);
        }

    }
}
