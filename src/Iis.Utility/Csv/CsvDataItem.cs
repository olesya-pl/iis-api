using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Utility.Csv
{
    public class CsvDataItem
    {
        public string Id { get; }
        public string ColumnName { get; }
        public string Value { get; }
        public CsvDataItem(string id, string columnName, string value)
        {
            Id = id;
            ColumnName = columnName;
            Value = value;
        }
    }
}
