using System;

namespace Iis.Utility
{
    public static class NullableHelper
    {
        public static void DoIfHasValue<T>(this T? p, Action<T> action) where T : struct
        {
            if (p.HasValue)
            {
                action(p.Value);
            }
        }
    }
}
