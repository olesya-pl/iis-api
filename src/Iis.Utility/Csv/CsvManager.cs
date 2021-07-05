using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Utility.Csv
{
    public class CsvManager
    {
        private List<string> _rows;
        private List<CsvDataItem> _items;
        private List<string> _columnNames;
        public CsvManager(IEnumerable<CsvDataItem> items)
        {
            _items = items
                .OrderBy(item => item.Id)
                .ThenBy(item => item.ColumnName)
                .ToList();

            Initialize();
        }
        private void Initialize()
        {
            _columnNames = ExtractColumnNames();
            _rows = ExtractRows();
        }
        private List<string> ExtractColumnNames()
        {
            string prevId = null, prevColumnName = null;
            int columnNameCounter = 0;
            var columnNames = new HashSet<string>();

            foreach (var item in _items)
            {
                columnNameCounter = item.Id == prevId && item.ColumnName == prevColumnName ?
                    columnNameCounter + 1 : 0;

                var indexedColumnName = GetIndexedColumnName(item.ColumnName, columnNameCounter);

                if (!columnNames.Contains(indexedColumnName))
                    columnNames.Add(indexedColumnName);

                prevId = item.Id;
                prevColumnName = item.ColumnName;
            }

            return columnNames.OrderBy(name => name).ToList();
        }

        private List<string> ExtractRows()
        {
            var rows = new List<string>();

            string prevId = null, prevColumnName = null;
            int columnNameCounter = 0;
            int lastCellIndex = 0;
            var sb = new StringBuilder();

            var columnNames = new HashSet<string>();

            foreach (var item in _items)
            {
                columnNameCounter = item.Id == prevId && item.ColumnName == prevColumnName ?
                    columnNameCounter + 1 : 0;

                if (item.Id != prevId)
                {
                    rows.Add(sb.ToString());
                    sb = new StringBuilder();
                    lastCellIndex = 0;
                }
                
                var indexedColumnName = GetIndexedColumnName(item.ColumnName, columnNameCounter);

                var columnIndex = _columnNames.IndexOf(indexedColumnName);

                for (int i = lastCellIndex; i < columnIndex; i++)
                    sb.Append(',');

                lastCellIndex = columnIndex;

                sb.Append(ToCsvFormat(item.Value));

                prevId = item.Id;
                prevColumnName = item.ColumnName;
            }

            if (sb.Length > 0) rows.Add(sb.ToString());

            return rows;
        }

        private string GetIndexedColumnName(string columnName, int index) =>
            index == 0 ? columnName : $"{columnName}__{index}";

        private string ToCsvFormat(string str) =>
            "\"" + str.Replace("\"", "\"\"") + "\"";

        public string GetCsv()
        {
            var sb = new StringBuilder();
            foreach (var columnName in _columnNames)
            {
                if (sb.Length > 0) sb.Append(',');
                sb.Append(ToCsvFormat(columnName));
            }

            foreach (var row in _rows)
                sb.AppendLine(row);

            return sb.ToString();
        }
    }
}
