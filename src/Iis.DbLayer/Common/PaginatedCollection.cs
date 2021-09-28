using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.Common
{
    public class PaginatedCollection<TEntity>
        where TEntity : BaseEntity
    {
        public IReadOnlyCollection<TEntity> Items { get; }
        public int TotalCount { get; }

        private PaginatedCollection(int totalCount, IReadOnlyCollection<TEntity> items)
        {
            TotalCount = totalCount;
            Items = items;
        }

        public static async Task<PaginatedCollection<TEntity>> CreateAsync(
            IQueryable<TEntity> source,
            int offset,
            int limit,
            CancellationToken cancellationToken)
        {
            int totalCount = await source.CountAsync(cancellationToken);
            var items = await source
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync(cancellationToken);

            return new PaginatedCollection<TEntity>(totalCount, items);
        }
    }
}