using System;
namespace Iis.Utility
{
    public static class DecimalExtensions
    {
        public static decimal Truncate(this decimal decimalNum, int precision)
        {
            var precisionDivider = (int) Math.Pow(10, precision);

            return Math.Truncate(decimalNum * precisionDivider) / precisionDivider;
        }
    }
}