using System;
using System.Threading.Tasks;

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

        public static async Task DoIfHasValueAsync<T>(this T? p, Func<T, Task> action) where T : struct
        {
            if (p.HasValue)
            {
                await action(p.Value);
            }
        }
    }
}
