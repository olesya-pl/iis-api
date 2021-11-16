using System;
namespace Iis.Interfaces.Elastic
{
    public class SortingParams
    {
        private const string ASC = "asc";
        private const string DESC = "desc";
        public string ColumnName { get; }
        public string Order { get; }
        public SortingParams(string columnName, string order)
        {
            ColumnName = columnName;

            Order = order switch
            {
                _ when ASC.Equals(order, StringComparison.OrdinalIgnoreCase) => ASC,
                _ when DESC.Equals(order, StringComparison.OrdinalIgnoreCase) => DESC,
                _ => ASC
            };
        }
        public static SortingParams Default => new SortingParams(null, null);
    }
}
