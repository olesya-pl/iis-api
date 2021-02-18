using Humanizer;
using System.Linq;
using System.Globalization;
using System.Text;
using System;
using System.Collections.Generic;

namespace Iis.Utility
{
    public static class StringExtensions
    {
        public static string Capitalize(this string value) 
        {
            return string.IsNullOrEmpty(value)
                ? value
                : $"{value.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture)}{value.Substring(1)}";
        } 

        public static string ToLowerCamelCase(this string value)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : $"{value.Substring(0, 1).ToLower(CultureInfo.InvariantCulture)}{value.Substring(1)}";
        }

        public static string ToUnderscore(this string str) => str.Underscore();

        public static string RemoveWhiteSpace(this string input) => RemoveSymbols(input, new HashSet<char> { ' ' });

        public static string RemoveNewLineCharacters(this string input) => RemoveSymbols(input, new HashSet<char> { '\n', '\r', '\t' });

        public static string RemoveSymbols(this string input, ISet<char> removeSymbols)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            if (!removeSymbols.Any()) throw new ArgumentNullException(nameof(removeSymbols));

            var builder = new StringBuilder();

            foreach (var ch in input)
            {
                if (removeSymbols.Contains(ch)) continue;

                builder.Append(ch);
            }
            return builder.ToString();
        }        

        public static string EscapeSymbols(this string input, ISet<char> escapePattern)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            if (!escapePattern.Any()) throw new ArgumentNullException(nameof(escapePattern));

            var builder = new StringBuilder();

            foreach (var ch in input)
            {
                if (escapePattern.Contains(ch))
                {
                    builder.Append('\\');
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }        
    }
}
