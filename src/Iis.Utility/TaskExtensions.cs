using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Utility
{
    public static class TaskExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<T> task)
        {
            yield return await task;
        }
    }
}
