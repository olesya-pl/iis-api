using Iis.DbLayer.MaterialDictionaries;
using Iis.Interfaces.Materials;
using System;
using System.ComponentModel;

namespace Iis.Services.Contracts.Extensions
{
    public static class SortingParamsExtensions
    {
        public static ListSortDirection? AsSortDirection(this SortingParams sortingParams)
        {
            if (string.IsNullOrWhiteSpace(sortingParams.Order))
                return default;
            if (sortingParams.Order.Equals(SortDirections.ASC, StringComparison.CurrentCultureIgnoreCase))
                return ListSortDirection.Ascending;
            if (sortingParams.Order.Equals(SortDirections.DESC, StringComparison.CurrentCultureIgnoreCase))
                return ListSortDirection.Descending;

            return default;
        }
    }
}