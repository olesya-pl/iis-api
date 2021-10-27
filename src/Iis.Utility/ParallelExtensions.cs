using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Iis.Utility
{
    public static class ParallelExtensions
    {
        private static readonly ExecutionDataflowBlockOptions Options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        public static async Task<IReadOnlyCollection<TResult>> ForEachAsync<TItem, TResult>(
            this IReadOnlyCollection<TItem> items,
            Func<TItem, Task<TResult>> action)
        {
            if (items.Count == 0) return Array.Empty<TResult>();

            var bag = new ConcurrentBag<TResult>();
            var block = new ActionBlock<TItem>(async _ =>
            {
                var result = await action(_);
                bag.Add(result);
            }, Options); ;

            foreach (var item in items)
            {
                block.Post(item);
            }

            block.Complete();
            await block.Completion;

            return bag.ToArray();
        }
    }
}