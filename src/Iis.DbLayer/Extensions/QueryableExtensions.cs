using Iis.DataModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Iis.DbLayer.Extensions
{
    public static class QueryableExtensions
    {
        private const string ParameterName = "parameter";
        private const string OrderByMethodName = "OrderBy";
        private const string OrderByDescendingMethodName = "OrderByDescending";

        /// <summary>
        /// Sorts the elements of a sequence in specified order according to a sort column.
        /// </summary>
        /// <remarks>
        /// When sorting column is not found, it will order by entity identifier.
        /// By default uses ascending order.
        /// </remarks>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="source">Source</param>
        /// <param name="sortColumn">Sort column</param>
        /// <param name="sortOrder">Sort direction</param>
        /// <returns>An <see cref="IOrderedQueryable{TEntity}"/>. whose elements are sorted according to a key.</returns>
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string sortColumn, ListSortDirection? sortDirection)
            where TEntity : BaseEntity
        {
            var type = typeof(TEntity);
            var propertyInfo = string.IsNullOrWhiteSpace(sortColumn)
                ? type.GetProperty(nameof(BaseEntity.Id))
                : type.GetProperty(sortColumn, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? type.GetProperty(nameof(BaseEntity.Id));
            var parameter = Expression.Parameter(type, ParameterName);
            var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var typeArguments = new Type[]
            {
                type,
                propertyInfo.PropertyType
            };
            var resultExpression = Expression.Call(
                typeof(Queryable),
                sortDirection.AsOrderMethodName(),
                typeArguments,
                source.Expression,
                Expression.Quote(orderByExpression));

            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }

        private static string AsOrderMethodName(this ListSortDirection? sortDirection) => sortDirection switch
        {
            ListSortDirection.Ascending => OrderByMethodName,
            ListSortDirection.Descending => OrderByDescendingMethodName,
            _ => OrderByMethodName
        };
    }
}