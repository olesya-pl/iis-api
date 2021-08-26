using System;
using System.Globalization;

namespace Iis.Utility
{
    public static class DateTimeExtensions
    {
        public static string ToString(this DateTime? dateTime, string format)
        {
            return dateTime.HasValue
                ? dateTime.Value.ToString(format)
                : null;
        }

        public static string ToString(this DateTime? dateTime,
            string format,
            IFormatProvider formatProvider)
        {
            return dateTime.HasValue
                ? dateTime.Value.ToString(format, formatProvider)
                : null;
        }

        public static DateTime? AsDateTime(this string dateTimeString,
            string format,
            IFormatProvider formatProvider,
            DateTimeStyles dateTimeStyles)
        {
            return !string.IsNullOrWhiteSpace(dateTimeString)
                ? (DateTime?)DateTime.ParseExact(dateTimeString, format, formatProvider, dateTimeStyles)
                : null;
        }
    }
}